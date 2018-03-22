﻿using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using AirTransit_Standard.Models;
using AirTransit_Standard.Repositories;
using AirTransit_Standard.Utilities;

[assembly: InternalsVisibleTo("AirTransit_Standard_Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace AirTransit_Standard.Services
{
    class RSAEncryptionService : IEncryptionService
    {
        private readonly IKeySetRepository _keySetRepository;
        private readonly Encoding _encoding;
        internal static readonly int KEY_SIZE = 1024;
            
        public RSAEncryptionService(IKeySetRepository keySetRepository, Encoding encoding)
        {
            this._keySetRepository = keySetRepository;
            this._encoding = encoding;
        }

        public string GenerateSignature(string content)
        {
            try
            {
                using (var rsa = new RSACryptoServiceProvider(KEY_SIZE))
                {
                    var contentBytes = this._encoding.GetBytes(content);
                    var clientKey = this._keySetRepository.GetKeySet();
                    rsa.FromXmlStringNetCore(clientKey.PrivateKey);
                    var signature = rsa.SignData(contentBytes, CryptoConfig.MapNameToOID("SHA256"));
                    return Convert.ToBase64String(signature);
                }
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        
        public bool VerifySignature(string signature, string signedData, Contact contact)
        {
            try
            {
                using (var rsa = new RSACryptoServiceProvider(KEY_SIZE))
                {
                    var clientKey = contact.PublicKey;
                    rsa.FromXmlStringNetCore(clientKey);
                    var signatureBytes = this._encoding.GetBytes(signature);
                    var signedDataBytes = Convert.FromBase64String(signedData);
                    
                    var hash = new SHA256Managed();

                    bool dataOK = rsa.VerifyData(signedDataBytes, CryptoConfig.MapNameToOID("SHA256"), signatureBytes);
                    var hashedData = hash.ComputeHash(signedDataBytes);
                    return rsa.VerifyHash(hashedData, CryptoConfig.MapNameToOID("SHA256"), signatureBytes);
                }
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
        
        public string Encrypt(string message, Contact contact)
        {
            byte[] messageBytes = _encoding.GetBytes(message);
            
            try
            {
                byte[] encryptedData;
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(KEY_SIZE))
                {
                    rsa.FromXmlStringNetCore(contact.PublicKey);
                    encryptedData = rsa.Encrypt(messageBytes, RSAEncryptionPadding.Pkcs1);
                }
                return Convert.ToBase64String(encryptedData);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    
        public string Decrypt(string encryptedMessage)
        {
            var key = this._keySetRepository.GetKeySet();
            var encryptedMessageBytes = Convert.FromBase64String(encryptedMessage);
            
            try
            {
                byte[] decryptedData;
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(KEY_SIZE))
                {
                    RSA.FromXmlStringNetCore(key.PrivateKey);
                    decryptedData = RSA.Decrypt(encryptedMessageBytes, RSAEncryptionPadding.Pkcs1);
                }
                return _encoding.GetString(decryptedData);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());
    
                return null;
            }
        }
    }
}