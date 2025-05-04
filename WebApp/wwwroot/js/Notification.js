/*
 * Manages real-time notifications using SignalR:
 * - Opens a hub connection to receive “NotificationUpdated” and “NotificationDismissed” events.
 * - Provides functions to load, render, dismiss, and remove notifications in the UI.
 * - Updates the notification count and relative time display.
 * - Got the inspiration from the official SignalR documentation and examples from Hans video.
 *- https://learn.microsoft.com/en-us/aspnet/signalr/
 */


const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

// When any notification is updated server-side, reload the full list
connection.on("NotificationUpdated", function () {
    loadNotifications();
});

// When a notification is dismissed server-side, remove it from the UI
connection.on("NotificationDismissed", function (notificationId) {
    removeNotification(notificationId);
});

// Start the SignalR connection
connection.start().catch(error => console.error(error));

/*
 * Sends a request to dismiss a specific notification,
 * then removes it from the DOM if successful.
 */
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

/*
 * Removes the notification element with the given ID from the page
 * and updates the notification count display.
 */
function removeNotification(notificationId) {
    const element = document.querySelector(`.notification-item[data-id='${notificationId}']`);
    if (element) {
        element.remove();
        updateNotificationCount();
    }
}

/*
 * Fetches the latest notifications from the server,
 * rebuilds the notification list in the DOM,
 * and updates counts and relative timestamps.
 */
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