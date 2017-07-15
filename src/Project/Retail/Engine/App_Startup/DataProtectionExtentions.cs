using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Sitecore.Demo.Retail.Project.Engine.App_Startup
{
    public static class DataProtectionExtentions
    {
        /// <summary>
        /// Setups the data protection storage and encryption protection type
        /// </summary>
        /// <param name="services">The services.</param>
        public static void ConfigureDataProtection(this IServiceCollection services, IConfigurationRoot configuration)
        {
            var builder = services.AddDataProtection();

            // Persist keys to a specific directory (should be a network location in distributed application)
            var pathToKeyStorage = configuration.GetSection("AppSettings:EncryptionKeyStorageLocation").Value;
            builder.PersistKeysToFileSystem(new DirectoryInfo(pathToKeyStorage));

            var protectionType = configuration.GetSection("AppSettings:EncryptionProtectionType").Value.ToUpperInvariant();
            switch (protectionType)
            {
                case "DPAPI-SID":
                    var storageSid = configuration.GetSection("AppSettings:EncryptionSID").Value.ToUpperInvariant();
                    //// Uses the descriptor rule "SID=S-1-5-21-..." to encrypt with domain joined user
                    builder.ProtectKeysWithDpapiNG($"SID={storageSid}", flags: DpapiNGProtectionDescriptorFlags.None);
                    break;
                case "DPAPI-CERT":
                    var storageCertificateHash = configuration.GetSection("AppSettings:EncryptionCertificateHash").Value.ToUpperInvariant();
                    //// Searches the cert store for the cert with this thumbprint
                    builder.ProtectKeysWithDpapiNG(
                        $"CERTIFICATE=HashId:{storageCertificateHash}",
                        flags: DpapiNGProtectionDescriptorFlags.None);
                    break;
                case "LOCAL":
                    //// Only the local user account can decrypt the keys
                    builder.ProtectKeysWithDpapiNG();
                    break;
                case "MACHINE":
                    //// All user accounts on the machine can decrypt the keys
                    builder.ProtectKeysWithDpapi(protectToLocalMachine: true);
                    break;
                default:
                    //// All user accounts on the machine can decrypt the keys
                    builder.ProtectKeysWithDpapi(protectToLocalMachine: true);
                    break;
            }
        }
    }
}
