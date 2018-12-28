$(document).ready(function () {

    $('.artworktypedocumentsdownload').on("click", function () {
        $('#artworkdetailsection').addClass('in');
        //$(this).removeClass('in');
        //window.location.hash = $(this).attr('id');
        //window.location.hash = '#existingAttachmentfrommodel';
        //$('#existingAttachmentfrommodel').focus();

    });
    //SessionUpdater.Setup('KeepSessionAlive');
    $(".sectionDetailType").change();

    BindUserTags("#artworkdetailsection");

    setTimeout(function () {
        $("div.card-body").removeClass("collapse");
        $("div.card-body").addClass("in");
    }, 1500);



    BindAttachment("FNLSamplePhoto", "AttachmentSamplePhoto");

    bindArtworkAttachments();
    $("input[name=Product]").on("change", function () {
        if ($(this).val() !== undefined && $(this).val() == 'Import') {
            $('.localProduct').hide();
        }
        else {
            $('.localProduct').show();
        }
    });

    if ($("#Product").val() == "Local") {
        $('.localProduct').show();
    } else if ($("#Product").val() == "Import") {
        $('.localProduct').hide();
    }

    $("#Product1").on("change", function () {
        if ($(this).attr("checked")) {
            $('.localProduct').show();
            $('#Product').val($(this).val());
        }
        else {
            $('.localProduct').hide();
        }

    }).change();

    $("#Product2").on("change", function () {
        if ($(this).attr("checked")) {
            $('.localProduct').hide();
            $('#Product').val($(this).val());
        }
        else {
            $('.localProduct').show();
        }

    }).change();


    $("input#BusinessUnit").off("change").on("change", function () {
        var BusinessUnitvalue = $("#BusinessUnit").val();
        if (BusinessUnitvalue != undefined) {
            $("#ProductCategory").html("<option value=''>Select</option>");

            $(ProductCategoryList).each(function (i, item) {
                if (item.BusinessUnit == BusinessUnitvalue) {
                    var opt = $("<option/>");
                    opt.text(item.Value + ' (' + item.Code + ')');
                    opt.attr("value", item.Value);
                    opt.appendTo("#ProductCategory");
                }
            });

            var selectedValue = $("#ProductCategory").attr("data-selected");
            if ($("#ProductCategory").find("option[value='" + selectedValue + "']").length > 0) {
                $("#ProductCategory").val(selectedValue).change();
            } else {
                $("#ProductCategory").val('').change();
            }
        }

    }).change();

    BindArtwork();

    $('#ArtworkTypeList').multiselect({
        includeSelectAllOption: false,
    });

    if ($.trim($("#ArtworkTypeList").attr("data-selected")) != "") {
        $('#ArtworkTypeList').multiselect('select', $("#ArtworkTypeList").attr("data-selected").split(","));
    }

    $("#ArtworkTypeList").on("change", function () {
        $('#ArtworkType').val($("#ArtworkTypeList").val());
    }).change();


    BindValueValidation();

    jQuery(document).on('click', '.IsNotApplicable', function () {
        SetValueOfCheckbox(this);

    });
});

function SetValueOfCheckbox(obj) {
    var id = jQuery(obj).attr('id').split('[')[1].split(']')[0];
    jQuery('#TempExistingAttachment' + id).show();
    if (jQuery(obj).is(':checked')) {
        jQuery(obj).attr('checked', true);
        jQuery('#TempExistingAttachment' + id).hide();
    }
}

function GetMultiselectValue(options) {
    var selected = '';
    options.each(function () {
        var label = ($(this).attr('label') !== undefined) ? $(this).attr('label') : $(this).text();
        selected += label + ",";
    });
    return selected.substr(0, selected.length - 1);
}

function ItemCodeAdded(ele, id, text) {

    ShowWaitDialog();
    AjaxCall({
        url: "/ViewArtwork/GetArtworkInfo?itemCode=" + id,
        httpmethod: "GET",
        sucesscallbackfunction: function (result) {
            var isValidentry = true;
            $("#existingAttachmentfrommodel").html("");
            $.each(result, function (key, value) {
                if ($('input[name="ListDetails[0].ItemId"]').val() == 0 && getQueryStringParameterByName("IsRetrieve") == "") {
                    if (key == "ReferenceNo") {
                        HideWaitDialog();
                        return;
                    }
                    $("span." + key).find("span").text(value);
                    $("span." + key).find("input").val(value);
                    $("span." + key).find("textarea").val(value);
                    if ($("#" + key).val() == '') {
                        $("#" + key).val(value);
                    }
                    if (key == "BusinessUnit" && value !== 'undefined') {
                        $("input#BusinessUnit").change();
                    }
                    if (key == "ProductCategory" && value !== 'undefined') {
                        $('select#ProductCategory option[value="' + value + '"]').attr('selected', true)
                        //$('input#ProductCategory').val($('select#ProductCategory option:selected').text());
                        $("span." + key).find("span").text($('input#ProductCategory').val());
                    }
                           

                    if (key == "Product" && value !== 'undefined' && value != null) {

                        if ($("#Product") && value == "Local") {
                            $('.localProduct').show();
                           // $('input[id*=Product]').removeAttr('checked');
                            $('input[id*=Product1]').attr('checked', value);
                        } else if ($("#Product") && value == "Import") {
                            $('.localProduct').hide();
                           // $('input[id*=Product]').removeAttr('checked');
                            $('input[id*=Product2]').attr('checked', value);
                        }
                    }
                    if (key == "VendorCode" && value != null && value != undefined && value != "") {
                        $("#VendorCode").parent().find('ul.token-input-list').remove();
                        $('#VendorCode').tokenInput(BASEPATHURL + "/ExistingArtwork/GetAllVendors", {
                            hintText: "Type and Select by Vendor Code",
                            minChars: 3,
                            tokenLimit: 1,
                            preventDuplicates: true,
                            animateDropdown: true,
                            tokenFormatter: function (item) { return "<li><p>" + item["name"] + "</p></li>" },
                            onAdd: function (item) {
                                VendorCodeAdded($(this), item.id, encodeURIComponent(item.name));
                            },
                            prePopulate: [
                                    { id: value, name: value }
                            ],
                            onDelete: function () {
                                VendorCodeRemoved($(this));
                            }
                        });
                    }
                }
                if (key == "ID" && value !== 'undefined') {
                    ShowWaitDialog();
                    AjaxCall({
                        url: "/ViewArtwork/GetArtworkTypeFiles?id=" + value,
                        httpmethod: "GET",
                        sucesscallbackfunction: function (result) {
                            ShowWaitDialog();
                            BindExistingAttachment(result);
                            HideWaitDialog();
                        }
                    });

                }
            });
        }

        //BindApprover();

    });
}

function isItemLockedOrnot(lockingDate) {
    var today = new Date();
    today.setHours(0, 0, 0, 0);
    var d = new Date(parseInt(lockingDate.substr(6)))
    d.setHours(0, 0, 0, 0);

    if (today > d) {
        return true;
    }
    else
        return false;
}

function ItemCodeRemoved(ele) {

    $("#ItemCode").tokenInput("clear");
    $("#ItemCode").val("");

    $(".ItemCode").find('span').not(".field-validation-error").text('');
    $(".ItemCode").find('input').val('');
    $(".ItemCode").find("textarea").val('');
    $('select#ProductCategory option[value=""]').attr('selected', true);
    //BindExistingAttachment(null);
    $("#existingAttachmentfrommodel").html("");
    $("#btn1").addClass('hide');
    $("#btn3").addClass('hide');
    $("#btn2").addClass('hide');
    $('[name="Product"]').removeAttr('checked');
    $("#VendorCode").tokenInput("clear");
    $("#VendorCode").val("");
    $("input[name='Product']").change();
    AjaxCall({
        url: "/ViewArtwork/ClearTempData",
        httpmethod: "GET",
        sucesscallbackfunction: function (result) {
            HideWaitDialog();
            $("#existingAttachmentfrommodel").html(result);
        }
    });
    ////$(".approver").each(function () {
    ////    var role = $(this).attr("data-dept").trim();
    ////    if (role != "Creator" && role != "BU Approver" && role != "ABSQ Team" && role != "ABSQ Approver") {
    ////        $("td[data-dept$='" + role + "']").find("span").text('NA');
    ////        $("td[data-dept$='" + role + "']").find("input.hiddenuser").val('');
    ////        $("td[data-dept$='" + role + "']").find("input.hiddenusername").val('');
    ////    }

    ////});
}

function BindExistingAttachment(result) {
    if (result && result != "[]") {

        $("#existingAttachmentfrommodel").html(result);
        if ($("#IsABSQUser").val() == 'False') {
            $("#existingAttachmentfrommodel").addClass("disabled");
            setTimeout(function () {
                DisableFormItems();
            }, 1000);
        }
    }
    else {
        $("#existingAttachmentfrommodel").html("");
    }

    var isfileavailable = $("#isfileavailable").val();
    if ($("#IsNewArtwork") && $("#IsNewArtwork").val() == "false") {


        if ((!isfileavailable || isfileavailable == "False") && $("#IsABSQUser").val() == 'False') {
            $("#btn2").removeClass('hide');
            $("#btn1").addClass('hide');
            $("#btn3").addClass('hide');

        }
            //else if ((!isfileavailable || isfileavailable == "False") && $("#IsABSQUser").val() == 'True') {
            //    $("#btn1").removeClass('hide');
            //    $("#btn2").addClass('hide');
            //}
        else if ($("#IsABSQUser").val() == 'True') {
            $("#btn1").removeClass('hide');
            $("#btn2").addClass('hide');
            $("#btn3").removeClass('hide');
        }
    }
    BindArtwork();

    bindArtworkAttachments();

}

function BindArtwork() {

    var artworkTypeOptions = $("#strArtworkType").val();


    $('#ArtworkTypeList').multiselect('deselectAll', false);
    $('#ArtworkTypeList').multiselect('updateButtonText');


    if ($.trim(artworkTypeOptions) != "") {
        $('#ArtworkTypeList').multiselect('select', artworkTypeOptions.split(","));
    }

    //var selectedOptions = $('#ArtworkTypeList option:selected');

    $('#ArtworkTypeList option').each(function () {
        !$(this).is(':selected')
        var input = $('.artworktypemultiselect').find('input[value="' + $(this).val() + '"]');
        //var input = $('input[value="' + $(this).val() + '"]');
        if (artworkTypeOptions) {
            var enableoption = artworkTypeOptions.split(",")
        }
    });

    if ($.trim($("#ArtworkTypeList").attr("data-selected")) != "") {
        $('#ArtworkTypeList').multiselect('select', $("#ArtworkTypeList").attr("data-selected").split(","));
    }


    $("#ArtworkTypeList").on("change", function () {
        $('#ArtworkType').val($("#ArtworkTypeList").val());
        var selectedOptions = $('#ArtworkTypeList option:selected');
        $("input[type='hidden'][name='StrArtworkTypeList']").val(GetMultiselectValue(selectedOptions));
        AjaxCall({
            url: "/ViewArtwork/AddArtworkType?artWorkTypeCodes=" + $("#ArtworkTypeList").val() + "&itemCode=" + $("#ItemCode").val(),
            httpmethod: "GET",
            sucesscallbackfunction: function (result) {
                $("#existingAttachmentfrommodel").html(result);
                bindArtworkAttachments();
                if ($("#IsABSQUser").val() == 'False') {
                    $("#existingAttachmentfrommodel").addClass("disabled");
                    setTimeout(function () {
                        DisableFormItems();
                    }, 1000);
                }
            }
        });
    }).change();



}

function bindArtworkAttachments() {
    $(".existingArtworkAttachments").each(function (index) {
        var divId = $(this).find('div').attr('id');
        var fileAttachmentId = $(this).find('input').attr('id');
        var strArtworkTypecode = fileAttachmentId;
        if (fileAttachmentId) {
            strArtworkTypecode = fileAttachmentId.substr(0, fileAttachmentId.indexOf('.'));
            strArtworkTypecode = strArtworkTypecode + ".ArtworkTypeCode";
            fileAttachmentId = fileAttachmentId.replace(/[!"#$%&'()*+,.\/:;<=>?@[\\\]^`{|}~]/g, "\\$&");
            strArtworkTypecode = strArtworkTypecode.replace(/[!"#$%&'()*+,.\/:;<=>?@[\\\]^`{|}~]/g, "\\$&");
            var fileSize = 20 * 1024 * 1024;  //20 MB
            BindAttachment(fileAttachmentId, divId, 2, $('input[name="' + strArtworkTypecode + '"]').val(), fileSize);
        }
    });
}
function getQueryStringParameterByName(name) {
    name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
        results = regex.exec(location.search);
    return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
}

function BindValueValidation() {
    $(".valuecheck").on("keyup change load", function () {
        var strError = '';
        //if (IsNullNumber($("#UnitCartonDimensionL").val(), 0) > IsNullNumber($("#MasterCartondimensionL").val(), 0)) {
        //    strError = "Unit Carton lenght should not be greater than Master Carton lenght.<br/>"
        //}
        //if (IsNullNumber($("#UnitCartonDimensionW").val(), 0) > IsNullNumber($("#MasterCartondimensionW").val(), 0)) {
        //    strError += "Unit Carton width should not be greater than Master Carton width.<br/>"
        //}
        //if (IsNullNumber($("#UnitCartonDimensionH").val(), 0) > IsNullNumber($("#MasterCartondimensionH").val(), 0)) {
        //    strError += "Unit Carton height should not be greater than Master Carton height.<br/>"
        //}
        if (IsNullNumber($("#NetWeight").val(), 0) > IsNullNumber($("#GrossWeight").val(), 0)) {
            $(".valuecheckmsg1").html("Net Weight should not be greater than Gross Weight.");
        }
        else {
            $(".valuecheckmsg1").html("");
        }
        $(".valuecheckmsg").html(strError);
        if (strError == '') {
            $(".valuecheckmsg").addClass("field-validation-valid");
            $(".valuecheckmsg").removeClass("field-validation-error");
        }
        else {
            $(".valuecheckmsg").removeClass("field-validation-valid");
            $(".valuecheckmsg").addClass("field-validation-error");
        }
    });
}

function VendorCodeAdded(ele, id, text) {

}

function VendorCodeRemoved(ele) {

}