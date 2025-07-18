﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.ShoppingCart;
using static Nop.Web.Models.Order.CustomerOrderListModel;

namespace Nop.Web.Models.Customer;

public partial record CustomerInfoModel : BaseNopModel
{
    public CustomerInfoModel()
    {
        AvailableTimeZones = new List<SelectListItem>();
        AvailableCountries = new List<SelectListItem>();
        AvailableStates = new List<SelectListItem>();
        AssociatedExternalAuthRecords = new List<AssociatedExternalAuthModel>();
        CustomerAttributes = new List<CustomerAttributeModel>();
        GdprConsents = new List<GdprConsentModel>();
        this.CpDiscounts = new List<CpDiscount>();
        this.CpOrders = new List<CpOrderModel>();
        this.Vendors = new List<VendorModel>();
        this.PetProfiles = new List<PetProfile>();
    }

    [DataType(DataType.EmailAddress)]
    [NopResourceDisplayName("Account.Fields.Email")]
    public string Email { get; set; }
    [DataType(DataType.EmailAddress)]
    [NopResourceDisplayName("Account.Fields.EmailToRevalidate")]
    public string EmailToRevalidate { get; set; }

    public bool CheckUsernameAvailabilityEnabled { get; set; }
    public bool AllowUsersToChangeUsernames { get; set; }
    public bool UsernamesEnabled { get; set; }
    [NopResourceDisplayName("Account.Fields.Username")]
    public string Username { get; set; }

    //form fields & properties
    public bool GenderEnabled { get; set; }
    [NopResourceDisplayName("Account.Fields.Gender")]
    public string Gender { get; set; }

    public bool NeutralGenderEnabled { get; set; }

    public bool FirstNameEnabled { get; set; }
    [NopResourceDisplayName("Account.Fields.FirstName")]
    public string FirstName { get; set; }
    public bool FirstNameRequired { get; set; }
    public bool LastNameEnabled { get; set; }
    [NopResourceDisplayName("Account.Fields.LastName")]
    public string LastName { get; set; }
    public bool LastNameRequired { get; set; }

    public bool DateOfBirthEnabled { get; set; }
    [NopResourceDisplayName("Account.Fields.DateOfBirth")]
    public int? DateOfBirthDay { get; set; }
    [NopResourceDisplayName("Account.Fields.DateOfBirth")]
    public int? DateOfBirthMonth { get; set; }
    [NopResourceDisplayName("Account.Fields.DateOfBirth")]
    public int? DateOfBirthYear { get; set; }
    public bool DateOfBirthRequired { get; set; }
    public DateTime? ParseDateOfBirth()
    {
        return CommonHelper.ParseDate(DateOfBirthYear, DateOfBirthMonth, DateOfBirthDay);
    }

    public bool CompanyEnabled { get; set; }
    public bool CompanyRequired { get; set; }
    [NopResourceDisplayName("Account.Fields.Company")]
    public string Company { get; set; }
    public bool StreetAddressEnabled { get; set; }
    public bool StreetAddressRequired { get; set; }
    [NopResourceDisplayName("Account.Fields.StreetAddress")]
    public string StreetAddress { get; set; }

    public bool StreetAddress2Enabled { get; set; }
    public bool StreetAddress2Required { get; set; }
    [NopResourceDisplayName("Account.Fields.StreetAddress2")]
    public string StreetAddress2 { get; set; }

    public bool ZipPostalCodeEnabled { get; set; }
    public bool ZipPostalCodeRequired { get; set; }
    [NopResourceDisplayName("Account.Fields.ZipPostalCode")]
    public string ZipPostalCode { get; set; }

    public bool CityEnabled { get; set; }
    public bool CityRequired { get; set; }
    [NopResourceDisplayName("Account.Fields.City")]
    public string City { get; set; }

    public bool CountyEnabled { get; set; }
    public bool CountyRequired { get; set; }
    [NopResourceDisplayName("Account.Fields.County")]
    public string County { get; set; }

    public bool CountryEnabled { get; set; }
    public bool CountryRequired { get; set; }
    [NopResourceDisplayName("Account.Fields.Country")]
    public int CountryId { get; set; }
    public IList<SelectListItem> AvailableCountries { get; set; }

    public bool StateProvinceEnabled { get; set; }
    public bool StateProvinceRequired { get; set; }
    [NopResourceDisplayName("Account.Fields.StateProvince")]
    public int StateProvinceId { get; set; }
    public IList<SelectListItem> AvailableStates { get; set; }

    public bool PhoneEnabled { get; set; }
    public bool PhoneRequired { get; set; }
    [DataType(DataType.PhoneNumber)]
    [NopResourceDisplayName("Account.Fields.Phone")]
    public string Phone { get; set; }

    public bool FaxEnabled { get; set; }
    public bool FaxRequired { get; set; }
    [DataType(DataType.PhoneNumber)]
    [NopResourceDisplayName("Account.Fields.Fax")]
    public string Fax { get; set; }

    public bool NewsletterEnabled { get; set; }
    [NopResourceDisplayName("Account.Fields.Newsletter")]
    public bool Newsletter { get; set; }

    //preferences
    public bool SignatureEnabled { get; set; }
    [NopResourceDisplayName("Account.Fields.Signature")]
    public string Signature { get; set; }

    //time zone
    [NopResourceDisplayName("Account.Fields.TimeZone")]
    public string TimeZoneId { get; set; }
    public bool AllowCustomersToSetTimeZone { get; set; }
    public IList<SelectListItem> AvailableTimeZones { get; set; }

    //EU VAT
    [NopResourceDisplayName("Account.Fields.VatNumber")]
    public string VatNumber { get; set; }
    public string VatNumberStatusNote { get; set; }
    public bool DisplayVatNumber { get; set; }
    public bool VatNumberRequired { get; set; }

    //external authentication
    [NopResourceDisplayName("Account.AssociatedExternalAuth")]
    public IList<AssociatedExternalAuthModel> AssociatedExternalAuthRecords { get; set; }
    public int NumberOfExternalAuthenticationProviders { get; set; }
    public bool AllowCustomersToRemoveAssociations { get; set; }

    public IList<CustomerAttributeModel> CustomerAttributes { get; set; }

    public IList<GdprConsentModel> GdprConsents { get; set; }

    // Counter point data
    public IList<CpDiscount> CpDiscounts { get; set; } // Frequent Buyer Discounts
    public IList<CpOrderModel> CpOrders { get; set; }
    public string ReceiveRewardsMethod { get; set; }
    public string RewardsNumber { get; set; }
    public string RewardsPointsGoal { get; set; }
    public string RewardsPointsEarned { get; set; }
    public string RewardsPointsToGo { get; set; }
    public string StateProvinceText { get; set; }
    public string FormattedPhoneNumber { get; set; }
    public CpPreferredStoreModel CpPreferredStore { get; set; }
    public CustomerAttributeModel InterestedIn { get; set; }
    public IList<VendorModel> Vendors { get; set; }
    public IList<PetProfile> PetProfiles { get; set; }

    // Recurring orders
    public RecurringOrdersModel RecurringOrders { get; set; }

    #region Nested classes

    public partial record AssociatedExternalAuthModel : BaseNopEntityModel
    {
        public string Email { get; set; }

        public string ExternalIdentifier { get; set; }

        public string AuthMethodName { get; set; }
    }

    #endregion
}

public record class CpOrderModel : BaseNopModel
{
    public CpOrderModel()
    {
        OrderLines = new List<CpOrderLine>();
    }

    public CpOrder Order { get; set; }
    public List<CpOrderLine> OrderLines { get; set; }
    public string PointsPendingText { get; set; }
    public string PointsText { get; set; }
    public string StoreNameText { get; set; }
}

public record class CpPreferredStoreModel : BaseNopModel
{
    public CpPreferredStoreModel() { }

    public string StoreName { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Zip { get; set; }
    public string Phone { get; set; }
    public string Description { get; set; }
}