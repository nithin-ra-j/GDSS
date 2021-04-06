$(document).ready(function () {
    //change totalVotes,data dynamically from backend
    $("span").hide();
    var totalVotes = $("#vote-count").text;
    var question = $(".question").text;
    //var data = [
    //    { opted: 0, optcolor: "#3e9a23", label: "Alpha Particle" },
    //    { opted: 2, optcolor: "#f2933a", label: "Beta Minus Particle" },
    //    { opted: 3, optcolor: "#31a4a3", label: "Beta Plus Particle" },
    //    { opted: 7, optcolor: "#4354f2", label: "Gamma Ray" },
    //    { opted: 18, optcolor: "#f32f3f", label: "Neutron" }
    //];
    var data = [];
    for (var i = 0; i < $(".poll-item").length; i++) {
        var sel = 'span.' + i;
        var voteLabel = $(sel).text;
        data.push({ opted: i, optcolor: "#033890", label: voteLabel });
    }

    $('.question').html(question);
    $.each(data, function (key, value) {
        //finding prev of button,wont work if html tag is placed to aothr lines
        $('button.' + key).prev().html(value.label).css("color", value.optcolor);
        $('button.' + key).css("background", value.optcolor);
        $('span.' + key).css("background", value.optcolor);
    });

    updateDisplay(totalVotes, data);

    $('button').on('click', function (evt) {
        var btnClass = $(this).attr('class');

        data[btnClass].opted += 1;

        totalVotes += 1;

        updateDisplay(totalVotes, data);

        $('button').hide();
        $("span").show();
    });
});

var updateDisplay = function (voteCount, dataValues) {
    $.each(dataValues, function (key, value) {
        voteCountVal = parseFloat(voteCount);
        optedCount = parseFloat(value.opted);
        var sel = 'span.' + key;

        percentVal = voteCountVal > 0 && optedCount > 0 ? (optedCount / voteCountVal) * 100 : 0;
        $(sel).width(percentVal + '%');
        optedCount > 0 ? $(sel).html(percentVal.toFixed(0) + '%') : $(sel).html('0%');
        $('#vote-count').html(voteCountVal + ' votes');
        optedCount = 0;
        percentVal = 0;
        voteCountVal = 0;

    });
};