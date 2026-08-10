using NovaCore.BuildingBlock.Domain.Attributes;

namespace NovaCore.BuildingBlock.Domain.Enums;

public enum MessageCode
{
    // System Messages (001-099)
    [MessageCode("001", "Request success")]
    Success = 1,
    [MessageCode("002", "Resource created successfully")]
    Created = 2,
    [MessageCode("003", "Resource updated successfully")]
    Updated = 3,
    [MessageCode("004", "Resource deleted successfully")]
    Deleted = 4,
    [MessageCode("005", "Operation completed successfully")]
    Ok = 5,
    [MessageCode("006", "Request accepted")]
    Accepted = 6,
    [MessageCode("007", "No content")]
    NoContent = 7,
    [MessageCode("008", "System error occurred")]
    SystemError = 8,
    [MessageCode("009", "Service is unavailable")]
    ServiceUnavailable = 9,
    [MessageCode("010", "Database error")]
    DatabaseError = 10,
    [MessageCode("011", "External service error")]
    ExternalServiceError = 11,
    [MessageCode("012", "Internal server error")]
    InternalServerError = 12,
    [MessageCode("013", "Operation timeout")]
    OperationTimeout = 13,

    // Validation Errors (100-199)
    [MessageCode("100", "Validation failed")]
    ValidationFailed = 100,
    [MessageCode("101", "Invalid input")]
    InvalidInput = 101,
    [MessageCode("102", "Required field is missing")]
    RequiredFieldMissing = 102,
    [MessageCode("103", "Invalid format")]
    InvalidFormat = 103,
    [MessageCode("104", "Invalid length")]
    InvalidLength = 104,
    [MessageCode("105", "Value out of range")]
    InvalidRange = 105,
    [MessageCode("106", "Duplicate entry")]
    DuplicateEntry = 106,

    // General Client Errors (200-299)
    [MessageCode("200", "Bad request")]
    BadRequest = 200,
    [MessageCode("201", "Resource not found")]
    NotFound = 201,
    [MessageCode("202", "Resource conflict")]
    Conflict = 202,
    [MessageCode("203", "Unprocessable entity")]
    UnprocessableEntity = 203,
    [MessageCode("204", "Too many requests")]
    TooManyRequests = 204,
    [MessageCode("205", "Request timeout")]
    RequestTimeout = 205,

    // Authentication & Authorization (300-399)
    [MessageCode("300", "Invalid credentials")]
    InvalidCredentials = 300,
    [MessageCode("301", "Token has expired")]
    TokenExpired = 301,
    [MessageCode("302", "Invalid token")]
    InvalidToken = 302,
    [MessageCode("303", "Insufficient permissions")]
    InsufficientPermissions = 303,
    [MessageCode("304", "Unauthorized access")]
    Unauthorized = 304,
    [MessageCode("305", "Access forbidden")]
    Forbidden = 305,
    [MessageCode("306", "Account is locked")]
    AccountLocked = 306,
    [MessageCode("307", "Account is disabled")]
    AccountDisabled = 307,
    [MessageCode("308", "Session has expired")]
    SessionExpired = 308,

    // Product Service (400-499)
    [MessageCode("400", "Product not found")]
    ProductNotFound = 400,
    [MessageCode("401", "Product already exists")]
    ProductAlreadyExists = 401,
    [MessageCode("402", "Invalid product data")]
    InvalidProductData = 402,
    [MessageCode("403", "Product is out of stock")]
    ProductOutOfStock = 403,
    [MessageCode("404", "Invalid price")]
    InvalidPrice = 404,
    [MessageCode("405", "Product category not found")]
    ProductCategoryNotFound = 405,
    [MessageCode("406", "Invalid product category")]
    InvalidProductCategory = 406,
    [MessageCode("407", "Product image upload failed")]
    ProductImageUploadFailed = 407,
    [MessageCode("408", "Invalid product SKU")]
    InvalidSKU = 408,

    // Inventory Service (500-599)
    [MessageCode("500", "Insufficient stock available")]
    InsufficientStock = 500,
    [MessageCode("502", "Invalid quantity")]
    InvalidQuantity = 502,
    [MessageCode("504", "Warehouse not found")]
    WarehouseNotFound = 504,
    [MessageCode("505", "Location not found")]
    LocationNotFound = 505,
    [MessageCode("506", "Stock adjustment failed")]
    StockAdjustmentFailed = 506,

    // Order Service (600-699)
    [MessageCode("600", "Order not found")]
    OrderNotFound = 600,
    [MessageCode("601", "Invalid order status")]
    InvalidOrderStatus = 601,
    [MessageCode("602", "Order is already cancelled")]
    OrderAlreadyCancelled = 602,
    [MessageCode("603", "Invalid payment method")]
    InvalidPaymentMethod = 603,
    [MessageCode("604", "Order cannot be modified")]
    OrderCannotBeModified = 604,
    [MessageCode("605", "Shipping address is required")]
    ShippingAddressRequired = 605,
    [MessageCode("606", "Invalid order items")]
    InvalidOrderItems = 606,

    // User Service (700-799)
    [MessageCode("700", "User not found")]
    UserNotFound = 700,
    [MessageCode("701", "User already exists")]
    UserAlreadyExists = 701,
    [MessageCode("702", "Invalid email format")]
    InvalidEmailFormat = 702,
    [MessageCode("703", "Password is too weak")]
    WeakPassword = 703,
    [MessageCode("704", "Email is not verified")]
    EmailNotVerified = 704,
    [MessageCode("705", "Email already verified")]
    EmailAlreadyVerified = 705,
    [MessageCode("706", "Verification token has expired")]
    VerificationTokenExpired = 706,
    [MessageCode("707", "Invalid phone number")]
    InvalidPhoneNumber = 707,
    [MessageCode("708", "User registration failed")]
    UserRegistrationFailed = 708,

    // Payment Service (800-899)
    [MessageCode("800", "Payment failed")]
    PaymentFailed = 800,
    [MessageCode("801", "Payment gateway error")]
    PaymentGatewayError = 801,
    [MessageCode("802", "Insufficient funds")]
    InsufficientFunds = 802,
    [MessageCode("803", "Payment method not supported")]
    PaymentMethodNotSupported = 803,
    [MessageCode("804", "Transaction declined")]
    TransactionDeclined = 804,
    [MessageCode("805", "Refund failed")]
    RefundFailed = 805,
    [MessageCode("806", "Invalid coupon code")]
    InvalidCouponCode = 806,

    // Shipping Service (900-999)
    [MessageCode("900", "Shipment not found")]
    ShipmentNotFound = 900,
    [MessageCode("901", "Invalid shipment status")]
    InvalidShipmentStatus = 901,
    [MessageCode("902", "Shipment is already cancelled")]
    ShipmentAlreadyCancelled = 902,
    [MessageCode("903", "Shipment manifest cannot be modified")]
    ShipmentManifestLocked = 903,
    [MessageCode("904", "Transportation not found")]
    TransportationNotFound = 904,
    [MessageCode("905", "Invalid transportation status")]
    InvalidTransportationStatus = 905,
    [MessageCode("906", "Transportation is already assigned")]
    TransportationAlreadyAssigned = 906,
    [MessageCode("907", "Shipping provider not found")]
    ShippingProviderNotFound = 907,
    [MessageCode("908", "Shipping provider is not active")]
    ShippingProviderInactive = 908,
    [MessageCode("909", "Invalid shipping address")]
    InvalidShippingAddress = 909,
    [MessageCode("910", "Transportation cost rule not found")]
    TransportationCostRuleNotFound = 910,
    [MessageCode("911", "Pickup not found")]
    PickupNotFound = 911,
    [MessageCode("912", "Delivery not found")]
    DeliveryNotFound = 912,
    [MessageCode("913", "Return shipment not found")]
    ReturnShipmentNotFound = 913,
    [MessageCode("914", "Carrier integration not found")]
    CarrierIntegrationNotFound = 914,
    [MessageCode("915", "Carrier integration is not configured")]
    CarrierIntegrationNotConfigured = 915,
}
