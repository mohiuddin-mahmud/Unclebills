using System.Data;
using System.Xml;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Polls;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Localization;

namespace NopValley.Plugins.Misc.Services.Customers;

/// <summary>
/// Customer service
/// </summary>
public partial class CustomerService : ICustomerService
{
    #region Fields

    protected readonly CustomerSettings _customerSettings;
    protected readonly IEventPublisher _eventPublisher;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly INopDataProvider _dataProvider;
    protected readonly IRepository<Address> _customerAddressRepository;
    protected readonly IRepository<BlogComment> _blogCommentRepository;
    protected readonly IRepository<Customer> _customerRepository;
    protected readonly IRepository<CustomerAddressMapping> _customerAddressMappingRepository;
    protected readonly IRepository<CustomerCustomerRoleMapping> _customerCustomerRoleMappingRepository;
    protected readonly IRepository<CustomerPassword> _customerPasswordRepository;
    protected readonly IRepository<CustomerRole> _customerRoleRepository;
    protected readonly IRepository<ForumPost> _forumPostRepository;
    protected readonly IRepository<ForumTopic> _forumTopicRepository;
    protected readonly IRepository<GenericAttribute> _gaRepository;
    protected readonly IRepository<NewsComment> _newsCommentRepository;
    protected readonly IRepository<Order> _orderRepository;
    protected readonly IRepository<ProductReview> _productReviewRepository;
    protected readonly IRepository<ProductReviewHelpfulness> _productReviewHelpfulnessRepository;
    protected readonly IRepository<PollVotingRecord> _pollVotingRecordRepository;
    protected readonly IRepository<ShoppingCartItem> _shoppingCartRepository;
    protected readonly IShortTermCacheManager _shortTermCacheManager;
    protected readonly IStaticCacheManager _staticCacheManager;
    protected readonly IStoreContext _storeContext;
    protected readonly ShoppingCartSettings _shoppingCartSettings;
    protected readonly TaxSettings _taxSettings;


    #endregion

    #region Ctor

    public CustomerService(CustomerSettings customerSettings,
        IEventPublisher eventPublisher,
        IGenericAttributeService genericAttributeService,
        INopDataProvider dataProvider,
        IRepository<Address> customerAddressRepository,
        IRepository<BlogComment> blogCommentRepository,
        IRepository<Customer> customerRepository,
        IRepository<CustomerAddressMapping> customerAddressMappingRepository,
        IRepository<CustomerCustomerRoleMapping> customerCustomerRoleMappingRepository,
        IRepository<CustomerPassword> customerPasswordRepository,
        IRepository<CustomerRole> customerRoleRepository,
        IRepository<ForumPost> forumPostRepository,
        IRepository<ForumTopic> forumTopicRepository,
        IRepository<GenericAttribute> gaRepository,
        IRepository<NewsComment> newsCommentRepository,
        IRepository<Order> orderRepository,
        IRepository<ProductReview> productReviewRepository,
        IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository,
        IRepository<PollVotingRecord> pollVotingRecordRepository,
        IRepository<ShoppingCartItem> shoppingCartRepository,
        IShortTermCacheManager shortTermCacheManager,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext,
        ShoppingCartSettings shoppingCartSettings,
        TaxSettings taxSettings)
    {
        _customerSettings = customerSettings;
        _eventPublisher = eventPublisher;
        _genericAttributeService = genericAttributeService;
        _dataProvider = dataProvider;
        _customerAddressRepository = customerAddressRepository;
        _blogCommentRepository = blogCommentRepository;
        _customerRepository = customerRepository;
        _customerAddressMappingRepository = customerAddressMappingRepository;
        _customerCustomerRoleMappingRepository = customerCustomerRoleMappingRepository;
        _customerPasswordRepository = customerPasswordRepository;
        _customerRoleRepository = customerRoleRepository;
        _forumPostRepository = forumPostRepository;
        _forumTopicRepository = forumTopicRepository;
        _gaRepository = gaRepository;
        _newsCommentRepository = newsCommentRepository;
        _orderRepository = orderRepository;
        _productReviewRepository = productReviewRepository;
        _productReviewHelpfulnessRepository = productReviewHelpfulnessRepository;
        _pollVotingRecordRepository = pollVotingRecordRepository;
        _shoppingCartRepository = shoppingCartRepository;
        _shortTermCacheManager = shortTermCacheManager;
        _staticCacheManager = staticCacheManager;
        _storeContext = storeContext;
        _shoppingCartSettings = shoppingCartSettings;
        _taxSettings = taxSettings;
    }

    #endregion

    /// <summary>
    /// Gets a customer
    /// </summary>
    /// <param name="rewardsNumber">Extra Value Card Rewards Number</param>
    /// <param name="phoneLast4">Last 4 digits of Customer Phone Number</param>
    /// <returns>A customer</returns>
    //public virtual Customer GetCustomerByRewardsCard(string rewardsNumber, string phoneLast4)
    //{
    //    if (string.IsNullOrWhiteSpace(rewardsNumber) || string.IsNullOrWhiteSpace(phoneLast4))
    //        return null;


    //    // prepare parameters
    //    var pRewardsNumber = _dataProvider.GetDbParameter();
    //    pRewardsNumber.ParameterName = "RewardsNumber";
    //    pRewardsNumber.Value = rewardsNumber;
    //    pRewardsNumber.DbType = DbType.String;

    //    var pPhoneLast4 = _dataProvider.GetParameter();
    //    pPhoneLast4.ParameterName = "PhoneLast4";
    //    pPhoneLast4.Value = phoneLast4;
    //    pPhoneLast4.DbType = DbType.String;

    //    var pCustomerId = _dataProvider.GetParameter();
    //    pCustomerId.ParameterName = "CustomerId";
    //    pCustomerId.Value = 0;
    //    pCustomerId.Direction = ParameterDirection.InputOutput;
    //    pCustomerId.DbType = DbType.Int32;

    //    var spResult = _dbContext.ExecuteStoredProcedureList<Customer>("RewardsCardLookup", pRewardsNumber, pPhoneLast4, pCustomerId);
    //    int customerId = Convert.ToInt32(pCustomerId.Value);
    //    if (customerId == 0)
    //        return null;
    //    else
    //        return _customerRepository.GetById(customerId);
    //}

    /// <summary>
    /// Get customer Id by Extra Value Card Number
    /// </summary>
    /// <param name="evcNumber">Extra Value Card Number</param>
    /// <returns>Customer Id</returns>
    //public virtual async Task<Customer> GetCustomerByRewardsCardAsync(string rewardsNumber)
    //{
    //    if (string.IsNullOrWhiteSpace(rewardsNumber))
    //        return null;

    //    var command = _dbContext.Database.GetDbConnection().CreateCommand();
    //    command.CommandText = "RewardsCardLookup2";
    //    command.CommandType = CommandType.StoredProcedure;

    //    command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter
    //    {
    //        ParameterName = "RewardsNumber",
    //        Value = rewardsNumber,
    //        SqlDbType = SqlDbType.NVarChar
    //    });

    //    var outputParam = new Microsoft.Data.SqlClient.SqlParameter
    //    {
    //        ParameterName = "CustomerId",
    //        SqlDbType = SqlDbType.Int,
    //        Direction = ParameterDirection.InputOutput,
    //        Value = 0
    //    };
    //    command.Parameters.Add(outputParam);

    //    await _dbContext.Database.OpenConnectionAsync();

    //    try
    //    {
    //        await command.ExecuteNonQueryAsync();
    //        int customerId = (int)outputParam.Value;
    //        return customerId == 0 ? null : await _customerRepository.GetByIdAsync(customerId);
    //    }
    //    finally
    //    {
    //        await _dbContext.Database.CloseConnectionAsync();
    //    }
    //}

}