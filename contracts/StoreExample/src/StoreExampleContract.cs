using System;
using System.ComponentModel;
using System.Numerics;

using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace StoreExample
{
    [DisplayName("Mariusz.StoreExampleContract")]
    [ManifestExtra("Author", "Mariusz")]
    [ManifestExtra("Email", "your@address.invalid")]
    [ManifestExtra("Description", "Describe your contract...")]
    public class StoreExampleContract : SmartContract
    {
        const byte Prefix_NumberStorage = 0x00;
        const byte Prefix_ContractOwner = 0xFF;
        private static Transaction Tx => (Transaction) Runtime.ScriptContainer;

        [DisplayName("NumberChanged")]
        public static event Action<UInt160, BigInteger> OnNumberChanged;

        public static object Main(string operation, params object[] args){
            if (Runtime.Trigger == TriggerType.Verification)
            {
                return true;
            }
            if (Runtime.Trigger == TriggerType.Application){
                switch (operation)
                {
                    case "query":
                            return Query((string)args[0]);
                    case "register":
                            return Register((string)args[0], (UInt160)args[1]);
                    case "delete":
                            return Delete((string)args[0]);
                    default:
                            return false;
                }
            }

            return false;
        }

        private static object Delete(string v)
        {
            throw new NotImplementedException();
        }

        private static object Query(string v)
        {
            throw new NotImplementedException();
        }

        public static bool ChangeNumber(BigInteger positiveNumber)
        {
            if (positiveNumber < 0)
            {
                throw new Exception("Only positive numbers are allowed.");
            }

            StorageMap contractStorage = new(Storage.CurrentContext, Prefix_NumberStorage);
            contractStorage.Put(Tx.Sender, positiveNumber);
            OnNumberChanged(Tx.Sender, positiveNumber);
            return true;
        }

        public static ByteString GetNumber()
        {
            StorageMap contractStorage = new(Storage.CurrentContext, Prefix_NumberStorage);
            return contractStorage.Get(Tx.Sender);
        }

        [DisplayName("_deploy")]
        public static void Deploy(object data, bool update)
        {
            if (update) return;

            var key = new byte[] { Prefix_ContractOwner };
            Storage.Put(Storage.CurrentContext, key, Tx.Sender);
        }

        private static bool Register(string domain, UInt160 owner){
            if (!Runtime.CheckWitness(owner))
                return false;
            var value = Storage.Get(Storage.CurrentContext, domain);
            if (value != null)
                return false;
            Storage.Put(Storage.CurrentContext, domain, owner);
            return true;
        }
        
        public static void Update(ByteString nefFile, string manifest)
        {
            var key = new byte[] { Prefix_ContractOwner };
            var contractOwner = (UInt160)Storage.Get(Storage.CurrentContext, key);

            if (!contractOwner.Equals(Tx.Sender))
            {
                throw new Exception("Only the contract owner can update the contract");
            }

            ContractManagement.Update(nefFile, manifest, null);
        }
    }
}
