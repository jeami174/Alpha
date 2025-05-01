const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

connection.on("NotificationUpdated", function () {
    loadNotifications();
});

connection.on("NotificationDismissed", function (notificationId) {
    removeNotification(notificationId);
});

connection.start().catch(error => console.error(error));

async function dismissNotification(notificationId) {
    try {
        const res = await fetch(`/api/notifications/dismiss/${notificationId}`, { method: 'POST' });
        if (res.ok) {
            removeNotification(notificationId);
        } else {
            console.error('Error removing notification');
        }
    } catch (error) {
        console.error('Error removing notification: ', error);
    }
}

function removeNotification(notificationId) {
    const element = document.querySelector(`.notification-item[data-id='${notificationId}']`);
    if (element) {
        element.remove();
        updateNotificationCount();
    }
}

async function loadNotifications() {
    try {
        const response = await fetch('/api/notifications');
        if (!response.ok) throw new Error('Failed to fetch notifications');

        const notifications = await response.json();
        const list = document.getElementById('notification-list');
        list.innerHTML = '';

        notifications.forEach(notification => {
            const item = document.createElement('div');
            item.className = 'notification-item';
            item.setAttribute('data-id', notification.id);

            item.innerHTML = `
                <img src="${notification.imagePath}" alt="User" class="notification-avatar" />
                <div class="notification-content">
                    <div class="notification-text">${notification.message}</div>
                    <div class="notification-time" data-created="${new Date(notification.created).toISOString()}"></div>
                </div>
                <button class="notification-close" onclick="dismissNotification('${notification.id}')">×</button>
            `;

            list.appendChild(item);
        });

        updateNotificationCount();
        updateRelativeTimes();
    } catch (err) {
        console.error('Error loading notifications:', err);
    }
}
