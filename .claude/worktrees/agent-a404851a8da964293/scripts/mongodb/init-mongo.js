// Initialize MongoDB databases and collections
db = db.getSiblingDB('admin');

// Create order_db
db = db.getSiblingDB('order_db');
db.createCollection('orders');
db.createCollection('order_items');

// Create indexes for better query performance
db.orders.createIndex({ 'customerId': 1 });
db.orders.createIndex({ 'status': 1 });
db.orders.createIndex({ 'createdAt': -1 });

// Initialize product_db if needed for product search/caching
db = db.getSiblingDB('product_db');
db.createCollection('products');
db.products.createIndex({ 'name': 'text' });
db.products.createIndex({ 'category': 1 });

// Initialize user_db if needed
db = db.getSiblingDB('user_db');
db.createCollection('users');
db.users.createIndex({ 'email': 1 }, { unique: true });

// Initialize log collections (optional - for audit logging)
db = db.getSiblingDB('audit_logs');
db.createCollection('logs');
db.logs.createIndex({ 'timestamp': -1 });
db.logs.createIndex({ 'service': 1, 'timestamp': -1 });

// Initialize notification_db (Notification service - MongoDB-backed like Audit).
// One collection per aggregate root - NotificationRuleTarget/NotificationCampaignTarget are
// embedded documents inside notification_rules/notification_campaigns, not separate collections.
db = db.getSiblingDB('notification_db');

db.createCollection('user_notifications');
db.user_notifications.createIndex({ 'userId': 1, 'createdAt': -1 });
db.user_notifications.createIndex({ 'userId': 1, 'status': 1 });

db.createCollection('notification_groups');
db.notification_groups.createIndex({ 'name': 1 });

db.createCollection('notification_templates');
db.notification_templates.createIndex({ 'channel': 1 });

db.createCollection('notification_rules');
db.notification_rules.createIndex({ 'eventType': 1 });

db.createCollection('notification_campaigns');
db.notification_campaigns.createIndex({ 'status': 1 });
db.notification_campaigns.createIndex({ 'nextExecutionAt': 1 });

db.createCollection('notification_channels');
db.notification_channels.createIndex({ 'channelType': 1 }, { unique: true });

db.createCollection('notification_dispatches');
db.notification_dispatches.createIndex({ 'status': 1, 'nextRetryAt': 1 });
db.notification_dispatches.createIndex({ 'createdAt': -1 });
