using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SymmetricEncryptionAlgorithms
{
    public partial class Form1 : Form
    {
        private byte[] key;
        private byte[] IV;
        private byte[] cipherBytes;
        List<CipherMode> cipherModesLocal = new List<CipherMode>() { CipherMode.ECB, CipherMode.CBC, CipherMode.CFB };

        private Dictionary<string, long> dictionaryCipherResults;
        private Dictionary<string, long> dictionaryDecipherResults;
        public Form1()
        {
            InitializeComponent();

            openFileDialog1.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";

            string[] cipherTypes = { "All", "DES", "3DES", "RC2", "Rijndael", "AES" };
            string[] cipherModes = { "ECB", "CBC", "CFB" };


            comboBoxMethods.Items.AddRange(cipherTypes);
            comboBoxMethods.SelectedIndex = 0;
            comboBoxMode.Items.AddRange(cipherModes);

            chartEncryption.Series.Clear();
            chartDecryption.Series.Clear();

            for (int i = 1; i < cipherTypes.Length; i++)
            {
                chartEncryption.Series.Add(cipherTypes[i]);
                chartEncryption.Series[cipherTypes[i]].XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Auto;
                chartEncryption.Series[cipherTypes[i]].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;

                chartDecryption.Series.Add(cipherTypes[i]);
                chartDecryption.Series[cipherTypes[i]].XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Auto;
                chartDecryption.Series[cipherTypes[i]].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
            }
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.Cancel)
            {
                string filename = openFileDialog1.FileName;
                string fileText = File.ReadAllText(filename);
                textBoxSource.Text = fileText;
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            SymmetricAlgorithm saDES = DES.Create();
            SymmetricAlgorithm sa3DES = TripleDES.Create();
            SymmetricAlgorithm saRC2 = RC2.Create();
            SymmetricAlgorithm saRijndael = Rijndael.Create();
            SymmetricAlgorithm saAES = Aes.Create();

            switch (comboBoxMethods.SelectedItem.ToString())
            {
                case "All":
                    RunCipher(new List<SymmetricAlgorithm> { saDES, sa3DES, saRC2, saRijndael, saAES }, cipherModesLocal, new List<string> { "DES", "3DES", "RC2", "Rijndael", "AES" });
                    break;
                case "DES":
                    RunCipher(new List<SymmetricAlgorithm> { saDES }, cipherModesLocal, new List<string> { "DES" });
                    break;
                case "3DES":
                    RunCipher(new List<SymmetricAlgorithm> { sa3DES }, cipherModesLocal, new List<string> { "3DES" });
                    break;
                case "RC2":
                    RunCipher(new List<SymmetricAlgorithm> { saRC2 }, cipherModesLocal, new List<string> { "RC2" });
                    break;
                case "Rijndael":
                    RunCipher(new List<SymmetricAlgorithm> { saRijndael }, cipherModesLocal, new List<string> { "Rijndael" });
                    break;
                case "AES":
                    RunCipher(new List<SymmetricAlgorithm> { saAES }, cipherModesLocal, new List<string> { "AES" });
                    break;
            }
        }

        private void RunCipher(List<SymmetricAlgorithm> symmetricAlgorithm, List<CipherMode> cipherModesLocal, List<string> symmetricAlgorithmList)
        {
            dictionaryCipherResults = new Dictionary<string, long>();
            dictionaryDecipherResults = new Dictionary<string, long>();
            chartEncryption.Series.Clear();
            chartDecryption.Series.Clear();
            textBoxEncrypted.Clear();
            textBoxDecrypted.Clear();
            for (int i = 0; i < symmetricAlgorithm.Count; i++)
            {
                for (int j = 0; j < cipherModesLocal.Count; j++)
                {
                    progressBar1.PerformStep();
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    symmetricAlgorithm[i].GenerateKey();
                    key = symmetricAlgorithm[i].Key;
                    symmetricAlgorithm[i].GenerateIV();
                    IV = symmetricAlgorithm[i].IV;
                    symmetricAlgorithm[i].Mode = cipherModesLocal[j];
                    symmetricAlgorithm[i].Padding = PaddingMode.PKCS7;

                    MemoryStream ms = new MemoryStream();
                    CryptoStream cs = new CryptoStream(ms, symmetricAlgorithm[i].CreateEncryptor(), CryptoStreamMode.Write);
                    byte[] plainbytes = Encoding.UTF8.GetBytes(textBoxSource.Text.ToCharArray());
                    cs.Write(plainbytes, 0, plainbytes.Length);
                    cs.Close();
                    cipherBytes = ms.ToArray();
                    ms.Close();
                    stopwatch.Stop();

                    string ciphedText = Encoding.UTF8.GetString(cipherBytes);
                    dictionaryCipherResults.Add(symmetricAlgorithmList[i] + " " + cipherModesLocal[j].ToString(), stopwatch.ElapsedTicks);
                    textBoxEncrypted.Text += $"{symmetricAlgorithmList[i]} {cipherModesLocal[j]}: {ciphedText}\r\n";

                    stopwatch = new Stopwatch();
                    stopwatch.Start();
                    symmetricAlgorithm[i].Key = key;
                    symmetricAlgorithm[i].IV = IV;
                    MemoryStream ms1 = new MemoryStream(cipherBytes);
                    CryptoStream cs1 = new CryptoStream(ms1, symmetricAlgorithm[i].CreateEncryptor(), CryptoStreamMode.Read);
                    byte[] plainbytes1 = new Byte[cipherBytes.Length];
                    cs1.Read(plainbytes1, 0, cipherBytes.Length);
                    cs1.Close();
                    ms1.Close();
                    stopwatch.Stop();

                    string deciphedText = Encoding.UTF8.GetString(plainbytes);
                    dictionaryDecipherResults.Add(symmetricAlgorithmList[i] + " " + cipherModesLocal[j], stopwatch.ElapsedTicks);
                    textBoxDecrypted.Text += $"{symmetricAlgorithmList[i]} {cipherModesLocal[j]}: {deciphedText}\r\n";
                }
            }
            int counter = 0;
            foreach (KeyValuePair<string, long> item in dictionaryCipherResults)
            {
                chartEncryption.Series.Add(item.Key);
                chartEncryption.Series[counter].Points.AddY(item.Value);
                counter++;
            }
            counter = 0;
            foreach (KeyValuePair<string, long> item in dictionaryDecipherResults)
            {
                chartDecryption.Series.Add(item.Key);
                chartDecryption.Series[counter].Points.AddY(item.Value);
                counter++;
            }
        }

        private void comboBoxMethods_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxMethods.SelectedItem.ToString() != "All")
            {
                comboBoxMode.SelectedIndex = 0;
            }
            else
            {
                comboBoxMode.SelectedIndex = -1;
            }
        }

        private void comboBoxMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            cipherModesLocal.Clear();
            comboBoxMode.Enabled = true;
            switch (comboBoxMode.SelectedItem != null ? comboBoxMode.SelectedItem.ToString() : "All")
            {
                case "All":
                    cipherModesLocal.Add(CipherMode.ECB);
                    cipherModesLocal.Add(CipherMode.CBC);
                    cipherModesLocal.Add(CipherMode.CFB);
                    comboBoxMode.SelectedIndex = -1;
                    comboBoxMode.Text = "";
                    comboBoxMode.Enabled = false;
                    break;
                case "ECB":
                    cipherModesLocal.Add(CipherMode.ECB);
                    break;
                case "CBC":
                    cipherModesLocal.Add(CipherMode.CBC);
                    break;
                case "CFB":
                    cipherModesLocal.Add(CipherMode.CFB);
                    break; 
            }
        }
    }
}
