var connection = new signalR.HubConnectionBuilder().withUrl("/stockrealtime").build();
connection.on("UpdateStockView", function () {
    LoadMarket();
});
connection.on("UpdateNotification", function (key) {
    var userKey = $('#user-key').val();
    if (userKey == key) {
        GetNotificationCount();
    }
});

$(function () {
    connection.start().then(function () {
        console.log('Realtime stock connected.');
    }).catch(function (err) {
        return console.error(err.toString());
    });

    GetNotificationCount();

    $(document).on('click', '.s-c', function (e) {
        OpenModalDetails(e.target.innerText, e.target.attributes['title'].nodeValue, e.target.attributes['fo'].nodeValue);
    });
});

function OpenToast(message) {
    $(document).Toasts('create', {
        title: 'Thông báo',
        body: message,
        delay: 5000,
        autohide: true,
        position: 'bottomRight'
    })
}

function GetNotificationCount() {
    $.ajax({
        cache: false,
        type: "GET",
        contentType: "application/json",
        dataType: 'json',
        url: "/user/newnotificationscount",
        success: function (data) {
            $('#user-notification-bell').text(data);
            if (data == '0') {
                $('#user-notification-bell').hide();
            } else {
                $('#user-notification-bell').show();
            }
        },
        error: function () {
            console.log('Get user notification count error.');
        }
    });
}

function UpdateDetailsOnChange() {
    var postData = JSON.stringify({
        Note: $('#stock-user-note').val(),
        Symbol: $('#stock-details-symbol').val()
    });
    $.ajax({
        cache: false,
        type: "POST",
        contentType: "application/json",
        dataType: 'json',
        url: "/home/userstockdetailssave",
        data: postData,
        success: function (data) {
            console.log(data);
        },
        error: function () {
            console.log('Save details stock on close error');
        }
    });
}

function OpenModalDetails(symbol, title, isFollow) {
    var modalTitle = symbol;
    if (title != '') {
        modalTitle += ' (' + title + ')';
    }
    if (isFollow == '') {
        $('#remove-follow').hide();
        $('#add-follow').hide();
    } else {
        if (isFollow == '1') {
            $('#remove-follow').show();
            $('#add-follow').hide();
        } else {
            $('#add-follow').show();
            $('#remove-follow').hide();
        }
    }
    $('#follow-symbol').val(symbol);
    $('#stock-details-title').text("Chi tiết - " + modalTitle);
    var urlChartAndPriceHistory = '/home/stockdetails?symbol=' + symbol;
    $('#stock-details-body').html('<iframe src="' + urlChartAndPriceHistory + '" class="embed-responsive-item" frameborder="0"></iframe>');
    $('#stock-details-link').prop('href', urlChartAndPriceHistory);
    $('#stock-details-modal').modal('toggle');
}

function ToggleFollow() {
    var symbol = $('#follow-symbol').val();
    $.ajax({
        cache: false,
        type: "GET",
        contentType: "application/json",
        dataType: 'json',
        url: "/home/togglefollow?symbol=" + symbol,
        success: function (data) {
            if (data.status == 1) {
                OpenToast('<span class="text-success">' + data.message + '</span>');
            }
            else {
                OpenToast('<span class="text-danger">' + data.message + '</span>');
            }
            $('#stock-details-modal').modal('toggle');
        },
        error: function () {
            console.log('ToggleFollow error');
        }
    });
}