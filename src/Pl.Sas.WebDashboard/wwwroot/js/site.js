function OpenToast(message) {
    $(document).Toasts('create', {
        title: 'Thông báo',
        body: message,
        delay: 5000,
        autohide: true,
        position: 'bottomRight'
    })
}