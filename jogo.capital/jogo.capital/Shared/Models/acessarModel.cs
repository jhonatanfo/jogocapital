using System;
using System.ComponentModel.DataAnnotations;

namespace jogo.capital.Shared.Models
{
	public class acessarModel
    {

        [Required]
        [Display(Name = "Tipo")]
        public string Tipo { get; set; } = "";

        [Required]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Documento")]
        public string Documento { get; set; } = "";

        [Required]
        [StringLength(100, ErrorMessage = "A {0} precisa conter um tamanho de {2} e {1}", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Senha { get; set; } = "";

    }
}

