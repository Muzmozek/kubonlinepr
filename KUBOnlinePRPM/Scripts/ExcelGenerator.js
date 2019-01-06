$(document).ready(function () {
	var modal = new Custombox.modal({
		content: {
			effect: 'fadein',
			target: '#UploadProgressModal'
		}
	});

	$(document).on("click", "#GenerateExcelFile", function (e) {
		e.preventDefault();
		var fd = new FormData();
		var data = $("#GenerateExcelPage").serializeArray();
		$.each(data, function (key, input) {
			fd.append(input.name, input.value);
		});
		$.ajax({
			url: URLAdmin,
			type: 'POST',
			data: fd,
			contentType: false,
			processData: false,
			cache: false,
			beforeSend: function () {
				modal.open();
			},
			xhr: function () { // Custom XMLHttpRequest
				var xhr = new window.XMLHttpRequest();
				//Upload progress
				xhr.upload.addEventListener("progress", function (evt) {
					if (evt.lengthComputable) {
						var percentComplete = evt.loaded / evt.total;
						//Do something with upload progress
						console.log(percentComplete);
						$('#uploadProgress > .progress-bar').attr("style", "width: " + percentComplete * 99 + "%");
						$('#uploadProgress > .progress-bar')[0].textContent = "" + percentComplete * 99 + "% Complete ";
					}
				}, false);
				return xhr;
			},
			dataType: "json",
			success: function (resp) {
				if (resp.success) {
					alert("Excel files successfully generated");
					//window.location = UrlPRTabs + "?PRId=" + PRId + "&PRType=" + POType;
					//$('#NewPO').html(resp.view);
					//$.fn.custombox('close');
					Custombox.modal.close();
					//$("#nav-4-1-primary-hor-center--PODetails").load(UrlPOTabs + ' #PODetailsTab');
				} else {
					alert("Exception occured. Please contact admin");
					Custombox.modal.close();
					//window.location = UrlPRTabs + "?PRId=" + PRId + "&PRType=" + POType;
					//$.post($("#UrlPOList").attr('href'), {
					//    PRId: resp.PRId,
					//    PRType: POType
					//}, function (resp) {
					//    alert("The PO has been submited");
					//    window.location = $("#UrlPOList").attr('href') + "?type=All";
					//});
				} 
			}
		});
	});
});