using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Aegis
{
    public partial class Form1 : Form
    {
        private Label lblFile;
        private TextBox txtFilePath;
        private Button btnBrowse;
        private Label lblPassword;
        private TextBox txtPassword;
        private CheckBox chkObfuscate;
        private Button btnEncrypt;
        private Button btnDecrypt;
        private Label lblStatus;

        private const string HEADER_TEXT = "Aegis protection ";

        private const int SALT_SIZE = 16;
        private const int NONCE_SIZE = 12;
        private const int TAG_SIZE = 16;
        private const int KEY_SIZE = 32;
        private const int PBKDF2_ITERATIONS = 100000;

        public Form1()
        {
            InitializeComponentCustom();
        }

        #region Construção da Interface do Usuário (UI)

        private void InitializeComponentCustom()
        {
            this.Text = "Aegis - Crypt & Safe Vault";
            this.Size = new Size(500, 310);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            lblFile = new Label { Text = "Arquivo:", Location = new Point(20, 20), AutoSize = true };
            txtFilePath = new TextBox { Location = new Point(20, 42), Width = 330, ReadOnly = true };
            btnBrowse = new Button { Text = "Buscar...", Location = new Point(360, 40), Width = 95, Height = 25 };
            btnBrowse.Click += BtnBrowse_Click;

            lblPassword = new Label { Text = "Senha Principal:", Location = new Point(20, 80), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(20, 102), Width = 435, UseSystemPasswordChar = true };

            chkObfuscate = new CheckBox
            {
                Text = "Ofuscar nome/extensão do arquivo (salva como .txt numérico)",
                Location = new Point(20, 140),
                AutoSize = true,
                Checked = true
            };

            btnEncrypt = new Button
            {
                Text = "Criptografar",
                Location = new Point(20, 180),
                Width = 210,
                Height = 35,
                BackColor = Color.LightSteelBlue
            };
            btnEncrypt.Click += BtnEncrypt_Click;

            btnDecrypt = new Button
            {
                Text = "Descriptografar",
                Location = new Point(245, 180),
                Width = 210,
                Height = 35,
                BackColor = Color.LightGray
            };
            btnDecrypt.Click += BtnDecrypt_Click;

            lblStatus = new Label
            {
                Text = "Aguardando ação...",
                Location = new Point(20, 230),
                AutoSize = true,
                ForeColor = Color.DimGray
            };

            this.Controls.Add(lblFile);
            this.Controls.Add(txtFilePath);
            this.Controls.Add(btnBrowse);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(chkObfuscate);
            this.Controls.Add(btnEncrypt);
            this.Controls.Add(btnDecrypt);
            this.Controls.Add(lblStatus);
        }

        #endregion

        #region Eventos da UI

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Todos os Arquivos (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = ofd.FileName;
                    lblStatus.Text = "Arquivo selecionado.";
                    lblStatus.ForeColor = Color.Black;
                }
            }
        }

        private void BtnEncrypt_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                string inputPath = txtFilePath.Text;
                string password = txtPassword.Text;

                string dir = Path.GetDirectoryName(inputPath);
                string originalName = Path.GetFileName(inputPath);
                string outputPath;

                if (chkObfuscate.Checked)
                {
                    string randomNum = new Random().Next(10000000, 99999999).ToString();
                    outputPath = Path.Combine(dir, $"{randomNum}.txt");
                }
                else
                {
                    outputPath = inputPath + ".aegis";
                }

                EncryptFile(inputPath, outputPath, password, originalName);

                lblStatus.Text = "Criptografado com sucesso!";
                lblStatus.ForeColor = Color.DarkGreen;
                MessageBox.Show($"Arquivo criptografado com sucesso!\nSalvo em: {outputPath}", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Erro na criptografia.";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show($"Erro ao criptografar: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDecrypt_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                string inputPath = txtFilePath.Text;
                string password = txtPassword.Text;

                DecryptFile(inputPath, password);

                lblStatus.Text = "Descriptografado com sucesso!";
                lblStatus.ForeColor = Color.DarkGreen;
            }
            catch (CryptographicException)
            {
                lblStatus.Text = "Falha na descriptografia.";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show("Senha incorreta ou arquivo corrompido/adulterado!", "Erro de Segurança", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Erro na descriptografia.";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show($"Erro ao descriptografar: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtFilePath.Text) || !File.Exists(txtFilePath.Text))
            {
                MessageBox.Show("Por favor, selecione um arquivo válido.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Por favor, insira a senha.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        #endregion

        #region Núcleo de Criptografia (AES-256-GCM + Caracteres Chineses)

        private void EncryptFile(string inputFilePath, string outputFilePath, string password, string originalFileName)
        {
            byte[] fileBytes = File.ReadAllBytes(inputFilePath);
            byte[] fileNameBytes = Encoding.UTF8.GetBytes(originalFileName);

            byte[] payload = new byte[4 + fileNameBytes.Length + fileBytes.Length];
            Array.Copy(BitConverter.GetBytes(fileNameBytes.Length), 0, payload, 0, 4);
            Array.Copy(fileNameBytes, 0, payload, 4, fileNameBytes.Length);
            Array.Copy(fileBytes, 0, payload, 4 + fileNameBytes.Length, fileBytes.Length);

            byte[] salt = RandomNumberGenerator.GetBytes(SALT_SIZE);
            byte[] nonce = RandomNumberGenerator.GetBytes(NONCE_SIZE);
            byte[] key = DeriveKey(password, salt);

            byte[] cipherText = new byte[payload.Length];
            byte[] tag = new byte[TAG_SIZE];

            using (AesGcm aesGcm = new AesGcm(key, TAG_SIZE))
            {
                aesGcm.Encrypt(nonce, payload, cipherText, tag);
            }

            byte[] rawCryptoData = new byte[SALT_SIZE + NONCE_SIZE + TAG_SIZE + cipherText.Length];
            Buffer.BlockCopy(salt, 0, rawCryptoData, 0, SALT_SIZE);
            Buffer.BlockCopy(nonce, 0, rawCryptoData, SALT_SIZE, NONCE_SIZE);
            Buffer.BlockCopy(tag, 0, rawCryptoData, SALT_SIZE + NONCE_SIZE, TAG_SIZE);
            Buffer.BlockCopy(cipherText, 0, rawCryptoData, SALT_SIZE + NONCE_SIZE + TAG_SIZE, cipherText.Length);

            StringBuilder chineseText = new StringBuilder();
            foreach (byte b in rawCryptoData)
            {
                chineseText.Append((char)(0x4E00 + b));
            }

            using (StreamWriter writer = new StreamWriter(outputFilePath, false, Encoding.UTF8))
            {
                writer.Write(HEADER_TEXT);
                writer.Write(chineseText.ToString());
            }
        }

        private void DecryptFile(string inputFilePath, string password)
        {
            string content = File.ReadAllText(inputFilePath, Encoding.UTF8);

            if (!content.StartsWith(HEADER_TEXT))
            {
                throw new InvalidDataException("Formato inválido ou o arquivo não possui a assinatura Aegis.");
            }

            string chinesePart = content.Substring(HEADER_TEXT.Length);

            byte[] rawCryptoData = new byte[chinesePart.Length];
            for (int i = 0; i < chinesePart.Length; i++)
            {
                rawCryptoData[i] = (byte)(chinesePart[i] - 0x4E00);
            }

            if (rawCryptoData.Length < SALT_SIZE + NONCE_SIZE + TAG_SIZE)
            {
                throw new InvalidDataException("Arquivo corrompido ou incompleto.");
            }

            byte[] salt = new byte[SALT_SIZE];
            byte[] nonce = new byte[NONCE_SIZE];
            byte[] tag = new byte[TAG_SIZE];
            byte[] cipherText = new byte[rawCryptoData.Length - (SALT_SIZE + NONCE_SIZE + TAG_SIZE)];

            Buffer.BlockCopy(rawCryptoData, 0, salt, 0, SALT_SIZE);
            Buffer.BlockCopy(rawCryptoData, SALT_SIZE, nonce, 0, NONCE_SIZE);
            Buffer.BlockCopy(rawCryptoData, SALT_SIZE + NONCE_SIZE, tag, 0, TAG_SIZE);
            Buffer.BlockCopy(rawCryptoData, SALT_SIZE + NONCE_SIZE + TAG_SIZE, cipherText, 0, cipherText.Length);

            byte[] key = DeriveKey(password, salt);
            byte[] payload = new byte[cipherText.Length];

            using (AesGcm aesGcm = new AesGcm(key, TAG_SIZE))
            {
                aesGcm.Decrypt(nonce, cipherText, tag, payload);
            }


            int fileNameLength = BitConverter.ToInt32(payload, 0);
            string originalFileName = Encoding.UTF8.GetString(payload, 4, fileNameLength);

            byte[] fileBytes = new byte[payload.Length - 4 - fileNameLength];
            Array.Copy(payload, 4 + fileNameLength, fileBytes, 0, fileBytes.Length);


            string outputDirectory = Path.GetDirectoryName(inputFilePath);
            string finalOutputPath = Path.Combine(outputDirectory, originalFileName);

            File.WriteAllBytes(finalOutputPath, fileBytes);
            MessageBox.Show($"Arquivo descriptografado e restaurado com sucesso!\nSalvo em: {finalOutputPath}", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private byte[] DeriveKey(string password, byte[] salt)
        {
            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, PBKDF2_ITERATIONS, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(KEY_SIZE);
            }
        }

        #endregion
    }
}