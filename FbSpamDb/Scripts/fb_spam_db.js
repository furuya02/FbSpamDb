var interval = 60; //１回に処理する件数

function AppendTable(action, userId,accessToken,max, start, spam, order) {
    req = $.ajax({
        type: 'POST',
        url: '/Home/' + action,
        data: {
            userId: userId,
            accessToken: accessToken,
            max: max,
            start: start,
            interval: interval,
            spam: spam,
            order: order
        },
        success:
            function (data) {
                var s = data.split("$");
                max = s[0];//最大値
                spam = s[1];//スパム数

                //アラート
                var alert = $("#alert");
                alert.html(s[2]); 
                if (spam != 0) {
                    alert.addClass("alert-danger");
                }

                //テーブル
                $("#result").before(s[3]); 
                AppendTable(action, userId,accessToken,max, start + interval, spam,order); //再帰処理
            },
        error://complete
            function (data, status, msg) {
                //alert(msg);
            }
    });
}