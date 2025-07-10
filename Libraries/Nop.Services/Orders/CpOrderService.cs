using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Data;

namespace Nop.Services.Orders;

public partial class CpOrderService : ICpOrderService
{
    #region Fields

    private readonly IRepository<CpOrder> _cpOrderRepository;
    private readonly IRepository<CpOrderLine> _cpOrderLineRepository;
    private readonly IEventPublisher _eventPublisher;

    #endregion

    #region Ctor

    public CpOrderService(IRepository<CpOrder> cpOrderRepository,
        IRepository<CpOrderLine> cpOrderLineRepository,
        IEventPublisher eventPublisher)
    {
        this._cpOrderRepository = cpOrderRepository;
        this._cpOrderLineRepository = cpOrderLineRepository;
        this._eventPublisher = eventPublisher;
    }

    #endregion

    #region Methods

    // Order

    public virtual async Task InsertCpOrder(CpOrder order)
    {
        if (order == null)
            throw new ArgumentNullException("order");

        await _cpOrderRepository.InsertAsync(order);

        _eventPublisher.EntityInserted(order);
    }

    public virtual async Task UpdateCpOrder(CpOrder order)
    {
        if (order == null)
            throw new ArgumentNullException("order");

        await _cpOrderRepository.UpdateAsync(order);

        //event notification
        _eventPublisher.EntityUpdated(order);
    }

    public virtual async Task DeleteCpOrder(CpOrder order)
    {
        if (order == null)
            throw new ArgumentNullException("order");

        await _cpOrderRepository.DeleteAsync(order);

        _eventPublisher.EntityDeleted(order);
    }

    public virtual async Task<CpOrder> GetCpOrderByOrderId(string orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            throw new ArgumentNullException("orderId");

        var query = _cpOrderRepository.Table;
        query = query.Where(o => o.OrderId == orderId);
        await Task.Yield();
        return query.FirstOrDefault();
    }

    public virtual async Task<IList<CpOrder>> GetCpOrdersByCustomerId(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentNullException("customerId");

        var query = _cpOrderRepository.Table;
        query = query.Where(o => o.CustomerId == customerId);
        await Task.Yield();

        return query.OrderByDescending(o => o.PurchaseDate).ToList();
    }

    // Order Line

    public virtual async Task InsertCpOrderLine(CpOrderLine orderLine)
    {
        if (orderLine == null)
            throw new ArgumentNullException("orderLine");

        await _cpOrderLineRepository.InsertAsync(orderLine);

    }

    public virtual async Task UpdateCpOrderLine(CpOrderLine orderLine)
    {
        if (orderLine == null)
            throw new ArgumentNullException("orderLine");

        await _cpOrderLineRepository.UpdateAsync(orderLine);
    }

    public virtual async Task DeleteCpOrderLine(CpOrderLine orderLine)
    {
        if (orderLine == null)
            throw new ArgumentNullException("orderLine");

        await _cpOrderLineRepository.DeleteAsync(orderLine);       
    }

    public virtual async Task<CpOrderLine> GetCpOrderLine(string orderId, string lineNumber)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            throw new ArgumentNullException("orderId");

        if (string.IsNullOrWhiteSpace(lineNumber))
            throw new ArgumentNullException("lineNumber");

        var query = _cpOrderLineRepository.Table;

        await Task.Yield();
        return query.Where(o => o.OrderId == orderId && o.LineNumber == lineNumber).FirstOrDefault();
    }

    public virtual async Task<IList<CpOrderLine>> GetCpOrderLines(string orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            throw new ArgumentNullException("orderId");

        var query = _cpOrderLineRepository.Table;
        query = query.Where(o => o.OrderId == orderId);

        //await Task.Yield();
        return await query.OrderBy(o => o.LineNumber).ToListAsync();
    }

    #endregion
}