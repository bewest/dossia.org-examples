using System;

namespace ExtremeSwank.OpenId.PlugIns.Extensions
{
    /// <summary>
    /// Common schema definitions for Attribute Exchange.
    /// Legacy suggestion from 4-2007.  Implemented by myopenid.com.
    /// </summary>
    public static class AttributeExchangeSchemaLegacy
    {
        /// <summary>
        /// Subject's alias or "screen" name 
        /// </summary>
        public static SchemaDef UserName
        {
            get { return new SchemaDef("UserName", new Uri("http://schema.openid.net/namePerson/friendly")); }
        }
        /// <summary>
        /// Full name of subject
        /// </summary>
        public static SchemaDef FullName { get { return new SchemaDef("FullName", new Uri("http://schema.openid.net/namePerson")); } }
        /// <summary>
        /// Honorific prefix for the subject's name
        /// </summary>
        public static SchemaDef NamePrefix { get { return new SchemaDef("NamePrefix", new Uri("http://schema.openid.net/namePerson/prefix")); } }
        /// <summary>
        /// First or given name of subject
        /// </summary>
        public static SchemaDef FirstName { get { return new SchemaDef("FirstName", new Uri("http://schema.openid.net/namePerson/first")); } }
        /// <summary>
        /// Last name or surname of subject
        /// </summary>
        public static SchemaDef LastName { get { return new SchemaDef("LastName", new Uri("http://schema.openid.net/namePerson/last")); } }
        /// <summary>
        /// Middle name(s) of subject
        /// </summary>
        public static SchemaDef MiddleName { get { return new SchemaDef("MiddleName", new Uri("http://schema.openid.net/namePerson/middle")); } }
        /// <summary>
        /// Suffix of subject's name
        /// </summary>
        public static SchemaDef NameSuffix { get { return new SchemaDef("NameSuffix", new Uri("http://schema.openid.net/namePerson/suffix")); } }
        /// <summary>
        /// Company name (employer)
        /// </summary>
        public static SchemaDef CompanyName { get { return new SchemaDef("CompanyName", new Uri("http://schema.openid.net/company/name")); } }
        /// <summary>
        /// Employee title
        /// </summary>
        public static SchemaDef JobTitle { get { return new SchemaDef("JobTitle", new Uri("http://schema.openid.net/company/title")); } }
        /// <summary>
        /// Date of birth
        /// </summary>
        public static SchemaDef BirthDate { get { return new SchemaDef("BirthDate", new Uri("http://schema.openid.net/birthDate")); } }
        /// <summary>
        /// Year of birth (four digits)
        /// </summary>
        public static SchemaDef BirthYear { get { return new SchemaDef("BirthYear", new Uri("http://schema.openid.net/birthDate/birthYear")); } }
        /// <summary>
        /// Month of birth (1-12)
        /// </summary>
        public static SchemaDef BirthMonth { get { return new SchemaDef("BirthMonth", new Uri("http://schema.openid.net/birthDate/birthMonth")); } }
        /// <summary>
        /// Day of birth (1-31)
        /// </summary>
        public static SchemaDef Birthday { get { return new SchemaDef("Birthday", new Uri("http://schema.openid.net/birthDate/birthday")); } }
        /// <summary>
        /// Main phone number (preferred)
        /// </summary>
        public static SchemaDef PhonePreferred { get { return new SchemaDef("PhoneDefault", new Uri("http://schema.openid.net/contact/phone/default")); } }
        /// <summary>
        /// Home phone number 
        /// </summary>
        public static SchemaDef PhoneHome { get { return new SchemaDef("PhoneHome", new Uri("http://schema.openid.net/contact/phone/home")); } }
        /// <summary>
        /// Business phone number
        /// </summary>
        public static SchemaDef PhoneWork { get { return new SchemaDef("PhoneWork", new Uri("http://schema.openid.net/contact/phone/business")); } }
        /// <summary>
        /// Cellular (or mobile) phone number
        /// </summary>
        public static SchemaDef PhoneMobile { get { return new SchemaDef("PhoneMobile", new Uri("http://schema.openid.net/contact/phone/cell")); } }
        /// <summary>
        /// Fax number 
        /// </summary>
        public static SchemaDef PhoneFax { get { return new SchemaDef("PhoneFax", new Uri("http://schema.openid.net/contact/phone/fax")); } }
        /// <summary>
        /// Home postal address: street number, name and apartment number
        /// </summary>
        public static SchemaDef HomeAddress { get { return new SchemaDef("HomeAddress", new Uri("http://schema.openid.net/contact/postalAddress/home")); } }
        /// <summary>
        /// Home postal address: supplementary information 
        /// </summary>
        public static SchemaDef HomeAddress2 { get { return new SchemaDef("HomeAddress2", new Uri("http://schema.openid.net/contact/postalAddressAdditional/home")); } }
        /// <summary>
        /// Home city name
        /// </summary>
        public static SchemaDef HomeCity { get { return new SchemaDef("HomeCity", new Uri("http://schema.openid.net/contact/city/home")); } }
        /// <summary>
        /// Home state or province name 
        /// </summary>
        public static SchemaDef HomeState { get { return new SchemaDef("HomeState", new Uri("http://schema.openid.net/contact/state/home")); } }
        /// <summary>
        /// Home country code in ISO.3166.1988 (alpha 2) format 
        /// </summary>
        public static SchemaDef HomeCountry { get { return new SchemaDef("HomeCountry", new Uri("http://schema.openid.net/contact/country/home")); } }
        /// <summary>
        /// Home postal code; region specific format
        /// </summary>
        public static SchemaDef HomePostalCode { get { return new SchemaDef("HomePostalCode", new Uri("http://schema.openid.net/contact/postalCode/home")); } }
        /// <summary>
        /// Business postal address: street number, name and apartment number 
        /// </summary>
        public static SchemaDef WorkAddress { get { return new SchemaDef("WorkAddress", new Uri("http://schema.openid.net/contact/postalAddress/business")); } }
        /// <summary>
        /// Business postal address: supplementary information
        /// </summary>
        public static SchemaDef WorkAddress2 { get { return new SchemaDef("WorkAddress2", new Uri("http://schema.openid.net/contact/postalAddressAdditional/business")); } }
        /// <summary>
        /// Business city name 
        /// </summary>
        public static SchemaDef WorkCity { get { return new SchemaDef("WorkCity", new Uri("http://schema.openid.net/contact/city/business")); } }
        /// <summary>
        /// Business state or province name
        /// </summary>
        public static SchemaDef WorkState { get { return new SchemaDef("WorkState", new Uri("http://schema.openid.net/contact/state/business State/Province")); } }
        /// <summary>
        /// Business country code in ISO.3166.1988 (alpha 2) format
        /// </summary>
        public static SchemaDef WorkCountry { get { return new SchemaDef("WorkCountry", new Uri("http://schema.openid.net/contact/country/business")); } }
        /// <summary>
        /// Business postal or zip code; region specific format
        /// </summary>
        public static SchemaDef WorkPostalCode { get { return new SchemaDef("WorkPostalCode", new Uri("http://schema.openid.net/contact/postalCode/business")); } }
        /// <summary>
        /// Internet SMTP email address as per RFC2822 
        /// </summary>
        public static SchemaDef Email { get { return new SchemaDef("Email", new Uri("http://schema.openid.net/contact/email")); } }
        /// <summary>
        /// AOL instant messaging service handle
        /// </summary>
        public static SchemaDef IMAim { get { return new SchemaDef("IMAim", new Uri("http://schema.openid.net/contact/IM/AIM")); } }
        /// <summary>
        /// ICQ instant messaging service handle 
        /// </summary>
        public static SchemaDef IMIcq { get { return new SchemaDef("IMIcq", new Uri("http://schema.openid.net/contact/IM/ICQ")); } }
        /// <summary>
        /// MSN instant messaging service handle
        /// </summary>
        public static SchemaDef IMMsn { get { return new SchemaDef("IMMsn", new Uri("http://schema.openid.net/contact/IM/MSN")); } }
        /// <summary>
        /// Yahoo! instant messaging service handle
        /// </summary>        
        public static SchemaDef IMYahoo { get { return new SchemaDef("IMYahoo", new Uri("http://schema.openid.net/contact/IM/Yahoo")); } }
        /// <summary>
        /// Jabber instant messaging service handle
        /// </summary>
        public static SchemaDef IMJabber { get { return new SchemaDef("IMJabber", new Uri("http://schema.openid.net/contact/IM/Jabber")); } }
        /// <summary>
        /// Skype instant messaging service handle
        /// </summary>
        public static SchemaDef IMSkype { get { return new SchemaDef("IMSkype", new Uri("http://schema.openid.net/contact/IM/Skype")); } }
        /// <summary>
        /// Web site URL
        /// </summary>
        public static SchemaDef UrlWebsite { get { return new SchemaDef("UrlWebsite", new Uri("http://schema.openid.net/contact/web/default")); } }
        /// <summary>
        /// Blog URL
        /// </summary>
        public static SchemaDef UrlBlog { get { return new SchemaDef("UrlBlog", new Uri("http://schema.openid.net/contact/web/blog")); } }
        /// <summary>
        /// LinkedIn URL
        /// </summary>        
        public static SchemaDef UrlLinkedIn { get { return new SchemaDef("UrlLinkedIn", new Uri("http://schema.openid.net/contact/web/Linkedin")); } }
        /// <summary>
        /// Amazon URL
        /// </summary>
        public static SchemaDef UrlAmazon { get { return new SchemaDef("UrlAmazon", new Uri("http://schema.openid.net/contact/web/Amazon")); } }
        /// <summary>
        /// Flickr URL
        /// </summary>
        public static SchemaDef UrlFlickr { get { return new SchemaDef("UrlFlickr", new Uri("http://schema.openid.net/contact/web/Flickr")); } }
        /// <summary>
        /// del.icio.us URL
        /// </summary>
        public static SchemaDef UrlDelicious { get { return new SchemaDef("UrlDelicious", new Uri("http://schema.openid.net/contact/web/Delicious")); } }
        /// <summary>
        /// Spoken name (web URL to media file)
        /// </summary>
        public static SchemaDef SpokenName { get { return new SchemaDef("SpokenName", new Uri("http://schema.openid.net/media/spokenname")); } }
        /// <summary>
        /// Audio greeting (web URL to media file)
        /// </summary>
        public static SchemaDef AudioGreeting { get { return new SchemaDef("AudioGreeting", new Uri("http://schema.openid.net/media/greeting/audio")); } }
        /// <summary>
        /// Video greeting (web URL to media file)
        /// </summary>
        public static SchemaDef VideoGreeting { get { return new SchemaDef("VideoGreeting", new Uri("http://schema.openid.net/media/greeting/video")); } }
        /// <summary>
        /// Image (web URL); unspecified dimension
        /// </summary>
        public static SchemaDef Image { get { return new SchemaDef("Image", new Uri("http://schema.openid.net/media/image/default")); } }
        /// <summary>
        /// Image (web URL) with equal width and height
        /// </summary>
        public static SchemaDef ImageSquare { get { return new SchemaDef("ImageSquare", new Uri("http://schema.openid.net/media/image/aspect11")); } }
        /// <summary>
        /// Image (web URL) 4:3 aspect ratio - landscape
        /// </summary>
        public static SchemaDef Image4To3 { get { return new SchemaDef("Image4To3", new Uri("http://schema.openid.net/media/image/aspect43")); } }
        /// <summary>
        /// Image (web URL) 3:4 aspect ratio - portrait
        /// </summary>
        public static SchemaDef Image3To4 { get { return new SchemaDef("Image3To4", new Uri("http://schema.openid.net/media/image/aspect34")); } }
        /// <summary>
        /// Image (web URL); favicon format as per FAVICON-W3C. The format for the image must be 16x16 pixels or 32x32 pixels, using either 8-bit or 24-bit colors. The format of the image must be one of PNG (a W3C standard), GIF, or ICO. 
        /// </summary>
        public static SchemaDef Favicon { get { return new SchemaDef("Favicon", new Uri("http://schema.openid.net/media/image/favicon")); } }
        /// <summary>
        /// Gender, either "M" or "F"
        /// </summary>
        public static SchemaDef Gender { get { return new SchemaDef("Gender", new Uri("http://schema.openid.net/person/gender")); } }
        /// <summary>
        /// Preferred language, as per RFC4646 
        /// </summary>
        public static SchemaDef Language { get { return new SchemaDef("Language", new Uri("http://schema.openid.net/pref/language")); } }
        /// <summary>
        /// Home time zone information (as specified in zoneinfo)
        /// </summary>
        public static SchemaDef TimeZone { get { return new SchemaDef("TimeZone", new Uri("http://schema.openid.net/pref/timezone")); } }
        /// <summary>
        /// Biography (text) 
        /// </summary>
        public static SchemaDef Biography { get { return new SchemaDef("Biography", new Uri("http://schema.openid.net/media/biography")); } }
    }
}
