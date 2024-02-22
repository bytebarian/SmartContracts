using ContractCreatorAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Neo;
using Neo.Network.RPC;
using Neo.Wallets;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Utility = Neo.Network.RPC.Utility;
using Neo.Cryptography.ECC;
using Neo.Network.P2P.Payloads;
using Neo.VM;

namespace ContractCreatorAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractController : ControllerBase
    {
        [HttpPost]
        public async Task<bool> Create(ContractCreator contract) 
        {
            if (contract is null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            // choose a neo node with rpc opened
            RpcClient client = new RpcClient(new Uri("http://localhost:50012"));

            // get the KeyPair of your accounts
            KeyPair sellerKey = Utility.GetKeyPair(contract.Seller);
            KeyPair buyerKey = Utility.GetKeyPair(contract.Buyer);
            KeyPair tokenKey = Utility.GetKeyPair(contract.Token);
            UInt160 sender = Contract.CreateSignatureContract(sellerKey.PublicKey).ScriptHash;
            UInt160 buyer = Contract.CreateSignatureContract(buyerKey.PublicKey).ScriptHash;
            UInt160 token = Contract.CreateSignatureContract(tokenKey.PublicKey).ScriptHash;

            //Contract multiContract = Contract.CreateMultiSigContract(2, new List<ECPoint>() { sellerKey.PublicKey, buyerKey.PublicKey });
            // get the scripthash of the multi-signature Contract
            //UInt160 multiAccount = multiContract.Script.ToScriptHash();

            // construct the script, in this example, we will transfer 1024 GAS to multi-sign account
            // in contract parameter, the amount type is BigInteger, so we need to muliply the contract factor
            UInt160 scriptHash = NativeContract.GAS.Hash;
            byte[] script = scriptHash.MakeScript("transfer", sender, token, 1024 * NativeContract.GAS.Factor, buyer);

            // add Signers, which is a collection of scripthashs that need to be signed
            Signer[] cosigners = new[] { new Signer { Scopes = WitnessScope.CalledByEntry, Account = sender } };

            // initialize the TransactionManager with rpc client and magic
            // fill the script and cosigners
            TransactionManager txManager = await new TransactionManagerFactory(client)
                .MakeTransactionAsync(script, cosigners).ConfigureAwait(false);
            // add signature and sign transaction with the added signature
            Transaction tx = await txManager.AddSignature(sellerKey).SignAsync().ConfigureAwait(false);

            // broadcasts the transaction over the Neo network.
            await client.SendRawTransactionAsync(tx).ConfigureAwait(false);

            // print a message after the transaction is on chain
            WalletAPI neoAPI = new WalletAPI(client);
            await neoAPI.WaitTransactionAsync(tx);

            return true;
        }
    }
}
