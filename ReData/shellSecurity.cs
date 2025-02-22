﻿using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;
using static System.Windows.Forms.MessageBoxOptions;

namespace ReData
{
    public class shellSecurity
    {
        #region hideInfo
        public static byte[] CipherMode(byte[] message, byte[] key)
        {
            byte[] output = new byte[message.Length];// выходное сообщение 
            byte[] temp = new byte[2];//для хранения двух субблоков
            for (int i = 0; i < message.Length-1; i++)
            {
                temp[0] = message[i]; temp[1] = message[i+1];
                temp = Feistel(temp, key);
                output[i] = temp[0]; output[i+1] = temp[1];
            }
            return output;
        }
        public static byte[] Feistel(byte[] data, byte[] key) // Фейстель
        {
            int countRound = key.Length - 1;// кол-во раундов
            int keyIndexRound = 0; // индекс раундового ключа
            byte Left = data[0], Right = data[1]; // делим субблок на правую и левую часть
            for (int i = 0; i < countRound; i++)
            {
                if (i < countRound - 1) // если i не последний элемент 
                {
                    byte temp = Left;
                    Left = Convert.ToByte(Right ^ F(Left, key[keyIndexRound]));
                    Right = temp;

                }
                keyIndexRound++;
            }
            data[0] = Left;
            data[1] = Right;
            return data;
        }
        private static byte F(byte A, byte B)// функция F 
        {
            BitArray Abit = new BitArray(new byte[] { A });
            BitArray Bbit = new BitArray(new byte[] { B });
            byte result = ConvertToByte(Abit.Xor(Bbit));
            return result;
        }
        private static byte ConvertToByte(BitArray fBits)//функция перевода бит в байт
        {
            byte[] _byte = new byte[1];
            fBits.CopyTo(_byte, 0);
            return _byte[0];
        }
        private static int Decoder(string line, byte[] key)
        {
            
            byte[] mess = Encoding.UTF8.GetBytes(line);
            string result = Encoding.UTF8.GetString(CipherMode(mess, key));
            try
            {
                Convert.ToInt32(result);
            }
            catch(FormatException)
            {
                MessageBox.Show(@"Файл конфигурации был изменен", "٩(ఠ益ఠ)۶",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return -1;
            }
            return Convert.ToInt32(result);;
        }
        #endregion

        #region Regestry

        private static int _iRegestry(string sS, string sS1)
        {
            sS = @"Software\" + sS + @"\" + sS1;
            Microsoft.Win32.RegistryKey clRegistryKey;
            clRegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(sS);
            if (clRegistryKey == null)
            {
                clRegistryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(sS);
            }
            else
            {
                clRegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(sS);
            }
            string[] sKeyNames = clRegistryKey.GetValueNames();
            if (sKeyNames.Length != 0)
            {  
                var temp =clRegistryKey.GetValue("isFirstStart");
                if (temp.Equals(true))
                {
                    clRegistryKey.SetValue("isFirstStart", false);
                    
                }
                clRegistryKey.Close();
                return 0;
            }
            else
            {
                clRegistryKey.SetValue("isFirstStart", true);
                clRegistryKey.Close();
                return 1;
            }
        }
        #endregion
        public static bool ShellSecurityCipher(byte[] key)
        {
            if (_iRegestry("ReData", "ffff")==1)
            {
                if (!File.Exists("config.conf"))
                {
                    using (StreamWriter wr = new StreamWriter("config.conf", false,Encoding.UTF8))
                    {
                        byte[] mess = Encoding.UTF8.GetBytes("10");
                        string newline = Encoding.UTF8.GetString(CipherMode(mess, key));
                        wr.WriteLine(newline);
                    }
                }
            }
            if (File.Exists("config.conf"))
            {
                string line ="";
                StreamReader sr = new StreamReader("config.conf",Encoding.UTF8);
                while (!sr.EndOfStream)
                    line += sr.ReadLine();
                sr.Close();
                if (!getInfo(line, key))
                {
                    return false;
                }
            }
            else
            {
                MessageBox.Show(@"Файл конфигурации не найден или был удален.", "Ой-ёй (×﹏×)",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private static bool getInfo(string line, byte[] key)
        {
            bool answer = true;
            int count = Decoder(line,key);
            if (count != 0)
            {
                count--;
                if (count < 0)
                {
                    return false;
                }
            }
            else
            {
                MessageBox.Show("Использование программы прекращенно",
                    caption:"Ошибка",MessageBoxButtons.OK, MessageBoxIcon.Error);
                answer = false;
                return answer;
            }
            using (StreamWriter sr = new StreamWriter("config.conf"))
            {
                byte[] mess = Encoding.Unicode.GetBytes(Convert.ToString(count));
                string newline = Encoding.UTF8.GetString(CipherMode(mess, key));
                sr.WriteLine(newline);
            }
            return answer;
        }
    }
}