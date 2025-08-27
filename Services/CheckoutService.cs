using Microsoft.EntityFrameworkCore;
using ReshamBazaar.Api.Data;
using ReshamBazaar.Api.DTOs;
using ReshamBazaar.Api.Models;
using Microsoft.AspNetCore.Identity;
using System.Text;

namespace ReshamBazaar.Api.Services;

public class CheckoutService : ICheckoutService
{
    private readonly AppDbContext _ctx;
    private readonly ICouponService _couponSvc;
    private readonly IEmailService _emailService;
    private readonly UserManager<ApplicationUser> _userManager;
    
    public CheckoutService(
        AppDbContext ctx, 
        ICouponService couponSvc,
        IEmailService emailService,
        UserManager<ApplicationUser> userManager)
    {
        _ctx = ctx;
        _couponSvc = couponSvc;
        _emailService = emailService;
        _userManager = userManager;
    }

    public async Task<OrderReadDto> CheckoutAsync(string userId, CheckoutRequestDto request, CancellationToken ct = default)
    {
       // Console.WriteLine($"CouponCode received: '{request.CouponCode}'");

        var items = await _ctx.CartItems.Include(ci => ci.Product).Where(ci => ci.UserId == userId).ToListAsync(ct);
        if (items.Count == 0) throw new InvalidOperationException("Cart is empty");

        decimal subtotal = 0;
        var order = new Order
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ShippingAddress = request.Address != null
    ? $"{request.Address.FullName}, {request.Address.Line1}" +
      $"{(!string.IsNullOrWhiteSpace(request.Address.Line2) ? ", " + request.Address.Line2 : "")}, " +
      $"{request.Address.City} - {request.Address.Pincode}, {request.Address.Phone}"
    : null

        };

        foreach (var ci in items)
        {
            if (ci.Product == null) continue;
            var lineTotal = ci.Product.Price * ci.Quantity;
            subtotal += lineTotal;
            order.Items.Add(new OrderItem
            {
                ProductId = ci.ProductId,
                ProductName = ci.Product.Name,
                UnitPrice = ci.Product.Price,
                Quantity = ci.Quantity
            });
        }

        order.Total = subtotal;

        // Apply coupon if any
        decimal discount = 0;
        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var validation = await _couponSvc.ValidateAsync(request.CouponCode!, subtotal, ct);
            if (validation.IsValid)
            {
                discount = validation.DiscountAmount;
                order.CouponCode = request.CouponCode!.Trim().ToUpperInvariant();
            }
        }
        order.Discount = discount;
        order.FinalTotal = subtotal - discount;

        await _ctx.Orders.AddAsync(order, ct);
        _ctx.CartItems.RemoveRange(items);
        await _ctx.SaveChangesAsync(ct);

        // Send order confirmation email
        try
        {
            Console.WriteLine($"[CheckoutService] Preparing to send order confirmation for order {order.Id}");
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                Console.WriteLine($"[CheckoutService] User with ID {userId} not found");
                return new OrderReadDto(
                    order.Id,
                    order.CreatedAt,
                    order.Total,
                    order.Discount,
                    order.FinalTotal,
                    order.CouponCode,
                    order.Status,
                    order.Items.Select(oi => new OrderItemReadDto(
                        oi.ProductId,
                        oi.ProductName,
                        oi.UnitPrice,
                        oi.Quantity
                    )).ToList()
                );
            }

            if (string.IsNullOrEmpty(user.Email))
            {
                Console.WriteLine($"[CheckoutService] User {user.Id} has no email address");
                return new OrderReadDto(
                    order.Id,
                    order.CreatedAt,
                    order.Total,
                    order.Discount,
                    order.FinalTotal,
                    order.CouponCode,
                    order.Status,
                    order.Items.Select(oi => new OrderItemReadDto(
                        oi.ProductId,
                        oi.ProductName,
                        oi.UnitPrice,
                        oi.Quantity
                    )).ToList()
                );
            }

            Console.WriteLine($"[CheckoutService] Sending email to {user.Email}");
            
            var emailContent = new StringBuilder();
            emailContent.AppendLine($"<h1>Thank you for your order, {user.UserName}!</h1>");
            emailContent.AppendLine("<p>Your order has been received and is being processed.</p>");
            emailContent.AppendLine($"<p><strong>Order ID:</strong> {order.Id}</p>");
            emailContent.AppendLine($"<p><strong>Order Date:</strong> {order.CreatedAt:dd MMM yyyy HH:mm}</p>");
            emailContent.AppendLine("<p><strong>Order Summary:</strong></p>");
            emailContent.AppendLine("<ul>");
            foreach (var item in order.Items)
            {
                emailContent.AppendLine($"<li>{item.Quantity}x {item.ProductName} - ₹{item.UnitPrice * item.Quantity:N2}</li>");
            }
            emailContent.AppendLine("</ul>");
            emailContent.AppendLine($"<p><strong>Subtotal:</strong> ₹{order.Total:N2}</p>");
            if (order.Discount > 0)
            {
                emailContent.AppendLine($"<p><strong>Discount:</strong> -₹{order.Discount:N2}</p>");
            }
            emailContent.AppendLine($"<p><strong>Total:</strong> ₹{order.FinalTotal:N2}</p>");
            emailContent.AppendLine("<p>We'll send you shipping confirmation when your order is on its way.</p>");
            emailContent.AppendLine("<p>Thank you for shopping with us!</p>");
            emailContent.AppendLine("<p>Best regards,<br/>ReshamBazaar Team</p>");

            var emailRequest = new EmailRequest(
                ToEmail: user.Email!,
                Subject: $"Order Confirmation - #{order.Id}",
                Body: emailContent.ToString(),
                IsBodyHtml: true
            );

            Console.WriteLine($"[CheckoutService] Created email request for order {order.Id}");
            await _emailService.SendEmailAsync(emailRequest);
            Console.WriteLine($"[CheckoutService] Email sent successfully for order {order.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CheckoutService] Failed to send order confirmation email for order {order.Id}");
            Console.WriteLine($"[CheckoutService] Error: {ex.Message}");
            Console.WriteLine($"[CheckoutService] Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[CheckoutService] Inner Exception: {ex.InnerException.Message}");
            }
        }

        return new OrderReadDto(
            order.Id,
            order.CreatedAt,
            order.Total,
            order.Discount,
            order.FinalTotal,
            order.CouponCode,
            order.Status,
            order.Items.Select(oi => new OrderItemReadDto(
                oi.ProductId,
                oi.ProductName,
                oi.UnitPrice,
                oi.Quantity
            )).ToList()
        );
    }
}
