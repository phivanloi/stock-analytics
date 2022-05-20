var connection = new signalR.HubConnectionBuilder().withUrl("/stockrealtime").build();
connection.on("UpdateStockView", function () {
    LoadMarket();
});

$(function () {
    connection.start().then(function () {
        console.log('Realtime stock connected.');
    }).catch(function (err) {
        return console.error(err.toString());
    });

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