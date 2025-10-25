using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using VendaFlex.Infrastructure.Interfaces;

namespace VendaFlex.Infrastructure.Services
{
    /// <summary>
    /// Implementação de gerenciamento de credenciais usando Windows Credential Manager.
    /// Utiliza a API nativa do Windows para armazenamento seguro.
    /// </summary>
    public class WindowsCredentialManager : ICredentialManager
    {
        private const string TargetName = "VendaFlex_RememberedUser";
        private readonly ILogger<WindowsCredentialManager> _logger;

        public WindowsCredentialManager(ILogger<WindowsCredentialManager> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public bool SaveRememberedUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogWarning("Tentativa de salvar username vazio ou nulo");
                return false;
            }

            IntPtr credentialBlobPtr = IntPtr.Zero;

            try
            {
                byte[] credentialBytes = Encoding.Unicode.GetBytes(username);
                credentialBlobPtr = Marshal.AllocHGlobal(credentialBytes.Length);
                Marshal.Copy(credentialBytes, 0, credentialBlobPtr, credentialBytes.Length);

                var credential = new CREDENTIAL
                {
                    Type = CRED_TYPE.GENERIC,
                    TargetName = TargetName,
                    UserName = username,
                    CredentialBlob = credentialBlobPtr,
                    CredentialBlobSize = (uint)credentialBytes.Length,
                    Persist = CRED_PERSIST.LOCAL_MACHINE,
                    AttributeCount = 0,
                    Attributes = IntPtr.Zero,
                    Comment = "VendaFlex - Username salvo para 'Lembrar-me'",
                    TargetAlias = null
                };

                bool result = CredWrite(ref credential, 0);

                if (result)
                {
                    _logger.LogInformation("Username '{Username}' salvo com sucesso no Credential Manager", username);
                }
                else
                {
                    int error = Marshal.GetLastWin32Error();
                    _logger.LogError("Falha ao salvar username no Credential Manager. Código de erro: {ErrorCode}", error);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar username '{Username}' no Credential Manager", username);
                return false;
            }
            finally
            {
                // Liberar memória não gerenciada
                if (credentialBlobPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(credentialBlobPtr);
                }
            }
        }

        /// <inheritdoc/>
        public string? GetRememberedUsername()
        {
            try
            {
                IntPtr credPtr;
                bool result = CredRead(TargetName, CRED_TYPE.GENERIC, 0, out credPtr);

                if (!result)
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error != ERROR_NOT_FOUND)
                    {
                        _logger.LogWarning("Falha ao ler credencial do Credential Manager. Código de erro: {ErrorCode}", error);
                    }
                    return null;
                }

                try
                {
                    var credential = Marshal.PtrToStructure<CREDENTIAL>(credPtr);
                    _logger.LogInformation("Username recuperado com sucesso do Credential Manager");
                    return credential.UserName;
                }
                finally
                {
                    CredFree(credPtr);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar username do Credential Manager");
                return null;
            }
        }

        /// <inheritdoc/>
        public bool ClearRememberedUsername()
        {
            try
            {
                bool result = CredDelete(TargetName, CRED_TYPE.GENERIC, 0);

                if (result)
                {
                    _logger.LogInformation("Username removido com sucesso do Credential Manager");
                }
                else
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error == ERROR_NOT_FOUND)
                    {
                        _logger.LogInformation("Nenhuma credencial encontrada para remover");
                        return true; // Considera sucesso se não havia nada para remover
                    }
                    _logger.LogWarning("Falha ao remover credencial. Código de erro: {ErrorCode}", error);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover username do Credential Manager");
                return false;
            }
        }

        /// <inheritdoc/>
        public bool HasRememberedUsername()
        {
            return !string.IsNullOrEmpty(GetRememberedUsername());
        }

        #region Windows API Imports

        private const int ERROR_NOT_FOUND = 1168;

        [DllImport("Advapi32.dll", SetLastError = true, EntryPoint = "CredWriteW", CharSet = CharSet.Unicode)]
        private static extern bool CredWrite([In] ref CREDENTIAL userCredential, [In] uint flags);

        [DllImport("Advapi32.dll", SetLastError = true, EntryPoint = "CredReadW", CharSet = CharSet.Unicode)]
        private static extern bool CredRead(
            string target,
            CRED_TYPE type,
            int reservedFlag,
            out IntPtr credentialPtr);

        [DllImport("Advapi32.dll", SetLastError = true, EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode)]
        private static extern bool CredDelete(string target, CRED_TYPE type, int flags);

        [DllImport("Advapi32.dll", SetLastError = true)]
        private static extern bool CredFree([In] IntPtr cred);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CREDENTIAL
        {
            public uint Flags;
            public CRED_TYPE Type;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string? TargetName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string? Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public uint CredentialBlobSize;
            public IntPtr CredentialBlob;
            public CRED_PERSIST Persist;
            public uint AttributeCount;
            public IntPtr Attributes;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string? TargetAlias;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string? UserName;
        }

        private enum CRED_TYPE : uint
        {
            GENERIC = 1,
            DOMAIN_PASSWORD = 2,
            DOMAIN_CERTIFICATE = 3,
            DOMAIN_VISIBLE_PASSWORD = 4,
            GENERIC_CERTIFICATE = 5,
            DOMAIN_EXTENDED = 6,
            MAXIMUM = 7,
            MAXIMUM_EX = 1007
        }

        private enum CRED_PERSIST : uint
        {
            SESSION = 1,
            LOCAL_MACHINE = 2,
            ENTERPRISE = 3
        }

        #endregion
    }
}
