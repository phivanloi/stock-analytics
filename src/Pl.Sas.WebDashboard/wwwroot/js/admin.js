$(function () {
    const srConnection = new signalR.HubConnectionBuilder().withUrl("/stockrealtime").withAutomaticReconnect().build();
    srConnection.on("UpdateStockView", () => { LoadMarket(); });
    srConnection.on("UpdateRealtimeView", (jsonData) => {
        var obj = JSON.parse(jsonData);
        $('#' + obj.smb + ' .c-epp').text(obj.cepp);
        $('#' + obj.smb + ' .c-eap').text(obj.ceap);
        $('#' + obj.smb + ' .c-mpp').text(obj.cmpp);
        $('#' + obj.smb + ' .c-map').text(obj.cmap);
        $('#' + obj.smb + ' .c-app').text(obj.capp);
        $('#' + obj.smb + ' .c-aap').text(obj.caap);
        $('#' + obj.smb + ' .c-bpp').text(obj.cbpp);
        $('#' + obj.smb + ' .c-bap').text(obj.cbap);
        $('#' + obj.smb + ' .c-lcp').text(obj.clcp);
        console.log(obj);
    });

    async function srStartListen() {
        try {
            await srConnection.start();
            console.log("Realtime stock connected.");
        } catch (err) {
            console.log(err);
            setTimeout(srStartListen, 5000);
        }
    };
    srConnection.onclose(async () => { await start(); });
    srStartListen();

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