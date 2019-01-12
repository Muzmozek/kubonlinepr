$(document).ready(function () {
    $("#SendMessage").click(function (e) {
        e.preventDefault();
        var selectedUserId = []; var Message = $("#MessageContent").val();
        $("#SelMultipleUser :selected").each(function (i, selected) {
            selectedUserId[i] = $(selected).val();
        });
        if (selectedUserId.length !== 0) {
            $.post(UrlSendMessage, {

                PRId: PRId,
                Message: Message,
                SelectedUserId: selectedUserId

            }, function (resp) {
                if (resp.success === true) {
                    alert(resp.message);
                    Custombox.modal.close();
                    $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab');
                } else {
                    windows.location = resp.url;
                }                
            });
        }
    });
});