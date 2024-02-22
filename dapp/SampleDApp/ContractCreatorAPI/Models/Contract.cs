using System.ComponentModel.DataAnnotations;

namespace ContractCreatorAPI.Models
{
    public class ContractCreator
    {
        [Required]
        public string Buyer { get; set; }
        [Required]
        public string Seller { get; set; }
        [Range (1, int.MaxValue)]
        public decimal Price { get; set; }
        [Required]
        public string Token { get; set; }
    }
}
