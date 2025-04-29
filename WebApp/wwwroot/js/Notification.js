const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

connection.on("RecieveNotification", function (notification) {
    const notificationsList = document.querySelector('#notification-list'); // FIXAT!

    const item = document.createElement('div');
    item.className = 'notification-item';
    item.setAttribute('data-id', notification.id);

    item.innerHTML = `
        <img src="${notification.imagePath}" alt="User" class="notification-avatar" />
        <div class="notification-content">
            <div class="notification-text">${notification.message}</div>
            <div class="notification-time" data-created="${new Date(notification.created).toISOString()}">${notification.created}</div>
        </div>
        <button class="notification-close" onclick="dismissNotification('${notification.id}')">×</button>
    `;

    notificationsList.insertBefore(item, notificationsList.firstChild);

    updateRelativeTimes();
    updateNotificationCount();
});

connection.on("NotificationDismissed", function (notificationId) {
    const element = document.querySelector(`.notification-item[data-id="${notificationId}"]`);
    if (element) {
        element.remove();
        updateNotificationCount();
    }
});

connection.start().catch(error => console.error(error));

// --- Hjälpfunktioner ---
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

