﻿using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

public static class StringExtensions
{
    public static string RemoveAccents(this string text)
    {
        return string.Concat(Regex.Replace(text, @"(?i)[\p{L}-[ña-z]]+",
                             m => m.Value.Normalize(NormalizationForm.FormD))
                     .Where(c => CharUnicodeInfo.GetUnicodeCategory(c)
                                != UnicodeCategory.NonSpacingMark
                                && !char.IsPunctuation(c)
                                ));
    }

    public static string RemoveSpaces(this string text)
    {
        return new string(text.ToCharArray()
            .Where(c => !Char.IsWhiteSpace(c))
            .ToArray());
    }
}
