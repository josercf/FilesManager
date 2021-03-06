﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FilesManager.Models.Login
{
    public class LoginViewModel
    {
        public int COD_Usuario { get; set; }

        [Required(ErrorMessage = "Informe o usuário.")]
        [DisplayName("Usuário:")]
        public string User { get; set; }

        [Required(ErrorMessage = "Informe a senha.")]
        [DisplayName("Senha:")]
        public string Password { get; set; }
    }
}
