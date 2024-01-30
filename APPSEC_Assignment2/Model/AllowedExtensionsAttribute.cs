using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Microsoft.AspNetCore.Http;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class AllowedExtensionsAttribute : ValidationAttribute
{
    private readonly string[] _extensions;

    public AllowedExtensionsAttribute(string[] extensions)
    {
        _extensions = extensions;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is not IFormFile file)
        {
            return ValidationResult.Success; // Not a file, so consider it valid
        }

        var extension = Path.GetExtension(file.FileName);

        if (extension != null && !_extensions.Contains(extension.ToLower()))
        {
            return new ValidationResult(GetErrorMessage());
        }

        return ValidationResult.Success;
    }

    public string GetErrorMessage()
    {
        return $"Invalid file type. Allowed types are: {string.Join(", ", _extensions)}";
    }
}

