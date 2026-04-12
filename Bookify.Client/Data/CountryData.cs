using Bookify.Client.Models.Common;
using System.Collections.Generic;

namespace Bookify.Client.Data;

public static class CountryData
{
    public static List<CountryModel> Countries = new()
    {
        new CountryModel { Name = "Algeria", DialCode = "+213", Iso3Code = "DZA" },
        new CountryModel { Name = "Argentina", DialCode = "+54", Iso3Code = "ARG" },
        new CountryModel { Name = "Bahrain", DialCode = "+973", Iso3Code = "BHR" },
        new CountryModel { Name = "Brazil", DialCode = "+55", Iso3Code = "BRA" },
        new CountryModel { Name = "Egypt", DialCode = "+20", Iso3Code = "EGY" },
        new CountryModel { Name = "France", DialCode = "+33", Iso3Code = "FRA" },
        new CountryModel { Name = "Germany", DialCode = "+49", Iso3Code = "DEU" },
        new CountryModel { Name = "Iraq", DialCode = "+964", Iso3Code = "IRQ" },
        new CountryModel { Name = "Italy", DialCode = "+39", Iso3Code = "ITA" },
        new CountryModel { Name = "Jordan", DialCode = "+962", Iso3Code = "JOR" },
        new CountryModel { Name = "Kuwait", DialCode = "+965", Iso3Code = "KWT" },
        new CountryModel { Name = "Lebanon", DialCode = "+961", Iso3Code = "LBN" },
        new CountryModel { Name = "Libya", DialCode = "+218", Iso3Code = "LBY" },
        new CountryModel { Name = "Malaysia", DialCode = "+60", Iso3Code = "MYS" },
        new CountryModel { Name = "Mauritania", DialCode = "+222", Iso3Code = "MRT" },
        new CountryModel { Name = "Morocco", DialCode = "+212", Iso3Code = "MAR" },
        new CountryModel { Name = "Oman", DialCode = "+968", Iso3Code = "OMN" },
        new CountryModel { Name = "Palestine", DialCode = "+970", Iso3Code = "PSE" },
        new CountryModel { Name = "Portugal", DialCode = "+351", Iso3Code = "PRT" },
        new CountryModel { Name = "Qatar", DialCode = "+974", Iso3Code = "QAT" },
        new CountryModel { Name = "Russian Federation", DialCode = "+7", Iso3Code = "RUS" },
        new CountryModel { Name = "Saudi Arabia", DialCode = "+966", Iso3Code = "SAU" },
        new CountryModel { Name = "Somalia", DialCode = "+252", Iso3Code = "SOM" },
        new CountryModel { Name = "Spain", DialCode = "+34", Iso3Code = "ESP" },
        new CountryModel { Name = "Sudan", DialCode = "+249", Iso3Code = "SDN" },
        new CountryModel { Name = "Syrian Arab Republic", DialCode = "+963", Iso3Code = "SYR" },
        new CountryModel { Name = "Tunisia", DialCode = "+216", Iso3Code = "TUN" },
        new CountryModel { Name = "Turkey", DialCode = "+90", Iso3Code = "TUR" },
        new CountryModel { Name = "United Arab Emirates", DialCode = "+971", Iso3Code = "UAE" },
        new CountryModel { Name = "United Kingdom", DialCode = "+44", Iso3Code = "GBR" },
        new CountryModel { Name = "United States", DialCode = "+1", Iso3Code = "USA" },
        new CountryModel { Name = "Yemen", DialCode = "+967", Iso3Code = "YEM" }
    };
}
