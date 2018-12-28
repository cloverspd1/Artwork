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
    BindAttachment("FNLSamplePhoto", "AttachmentSamplePhoto");
    BindAttachment("FNLTechnicalSpecificationAttachment", "AttachmentTechnicalSpecification");
    BindAttachment("FNLQAPAttachment", "AttachmentQAP");
    BindAttachment("FNLADBLevel1Attachment", "AttachmentADBLevel1");
    BindAttachment("FNLADBLevel2Attachment", "AttachmentADBLevel2");
    BindAttachment("FNLMarketingLevel1Attachment", "AttachmentMarketingLevel1");
    BindAttachment("FNLMarketingLevel2Attachment", "AttachmentMarketingLevel2");
    BindAttachment("FNLQALevel1Attachment", "AttachmentQALevel1");
    BindAttachment("FNLQALevel2Attachment", "AttachmentQALevel2");
    BindAttachment("FNLSCMLevel1Attachment", "AttachmentSCMLevel1");
    BindAttachment("FNLSCMLevel2Attachment", "AttachmentSCMLevel2");
    BindAttachment("FNLABSQApprover", "AttachmentABSQApprover");
    bindArtworkAttachments();

    BindApprover();

    $("input#BusinessUnit").off("change").on("change", function () {
        var BusinessUnitvalue = $("#BusinessUnit option:selected").val();
        if (BusinessUnitvalue != undefined) {
            $("#ProductCategory").html("<option value=''>Select</option>");
            $(ProductCategoryList).each(function (i, item) {
                if (item.BusinessUnit == BusinessUnitvalue) {
                    var opt = $("<option/>");
                    ////opt.text(item.Value + ' - ' + item.Title);
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

    if ($("#Product").val() == "Local") {
        $('.localProduct').show();
    } else if ($("#Product").val() == "Import") {
        $('.localProduct').hide();
    }
    $("#Product1").on("change", function () {
        if ($(this).attr("checked")) {
            $('.localProduct').show();
            if ($(this).val() != '')
                $('#Product').val($(this).val());
        }
        else {
            $('.localProduct').hide();
        }

    }).change();

    $("#Product2").on("change", function () {
        if ($(this).attr("checked")) {
            $('.localProduct').hide();
            if ($(this).val() != '')
                $('#Product').val($(this).val());
        }
        else {
            $('.localProduct').show();
        }
    }).change();
});

function GetMultiselectValue(options) {
    var selected = '';
    options.each(function () {
        var label = ($(this).attr('label') !== undefined) ? $(this).attr('label') : $(this).text();
        selected += label + ",";
    });
    return selected.substr(0, selected.length - 1);
}

function ItemCodeAdded(ele, id, text) {
    if ($("#ReferenceNo").val() != '') {
        return;
    }
    ShowWaitDialog();
    AjaxCall({
        url: "/ExistingArtwork/GetArtworkInfo?itemCode=" + id,
        httpmethod: "GET",
        sucesscallbackfunction: function (result) {
            var isValidentry = true;
            if (result.LockingDate) {
                if ($("#WorkflowStatus").val() != "Completed" && $("#WorkflowStatus").val() != "Rejected") {
                    isValidentry = isItemLockedOrnot(result.LockingDate);
                }
            }
            if (isValidentry) {
                CheckInprogress(id);
                $.each(result, function (key, value) {
                    if ($('input[name="ListDetails[0].ItemId"]').val() == 0 && getQueryStringParameterByName("IsRetrieve") == "") {
                        if (key == "ReferenceNo") {
                            HideWaitDialog();
                            return;
                        }
                        $("span." + key).find("span").text($.trim(value));
                        $("span." + key).find("input").val($.trim(value));


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

                        $("span." + key).find("textarea").val($.trim(value));
                        if ($("#" + key).val() == '') {
                            $("#" + key).val($.trim(value));
                        }
                        if (key == "ProductCategory" && value !== 'undefined') {
                            $('select#ProductCategory option[value="' + value + '"]').attr('selected', true)
                            //  $('input#ProductCategory').val($('select#ProductCategory option:selected').text());
                            $("span." + key).find("span").text($('input#ProductCategory').val());
                        }
                        if (key == "Product" && value !== 'undefined' && value != null) {

                            if ($("#Product") && value == "Local") {
                                $('.localProduct').show();
                                $('input[id*=Product1]').attr('checked', value);
                                //$('#Product').val(value);
                                $("#Product1").change();

                            } else if ($("#Product") && value == "Import") {
                                $('.localProduct').hide();
                                $('input[id*=Product2]').attr('checked', value);
                                $("#Product2").change();                                                              
                            }
                        }

                    }
                    if (key == "ID" && value !== 'undefined') {
                        ShowWaitDialog();
                        AjaxCall({
                            url: "/ExistingArtwork/GetArtworkTypeFiles?id=" + $.trim(value),
                            httpmethod: "GET",
                            sucesscallbackfunction: function (result) {
                                ShowWaitDialog();
                                BindExistingAttachment(result);
                                HideWaitDialog();
                            }
                        });
                    }
                });
                //code
                if ($("#RequesterBusinessUnit").val() == "ADB" || $("#RequesterBusinessUnit").val() == "QA" || $("#RequesterBusinessUnit").val() == "SCM") {
                    BindMarkettingUser();
                }
            } else {
                HideWaitDialog();
                $('#ItemCode').val('');
                var date = '';
                if (result.LockingDate) {
                    var dateString = result.LockingDate.substr(6);
                    var currentTime = new Date(parseInt(dateString));
                    var month = currentTime.getMonth() + 1;
                    var day = currentTime.getDate();
                    var year = currentTime.getFullYear();
                    date = day + "/" + month + "/" + year;
                }
                var errMessage = "Selected Item code will be available for modification after " + date;
                AlertModal('Validation', errMessage);
            }
        }
    });
    $("#SuggestedBy").blur();
}

function VendorCodeAdded(ele, id, text) {
    //    $(ele).tokenInput("add");
    //    //$(ele).val(decodeURIComponent(text));
}

function isItemLockedOrnot(lockingDate) {
    var today = new Date();
    today.setHours(0, 0, 0, 0);
    var d = new Date(parseInt(lockingDate.substr(6)))
    d.setHours(0, 0, 0, 0);
    if (today > d) {
        return true;
    } else
        return false;
}

function ItemCodeRemoved(ele) {
    $("#ItemCode").tokenInput("clear");
    $("#ItemCode").val("");
    $(".ItemCode").find('span').text('');
    $(".ItemCode").find('input').val('');
    $(".ItemCode").find("textarea").val('');
    $('select#ProductCategory option[value=""]').attr('selected', true);
    $('[name="Product"]').removeAttr('checked');

    $('#Product1').val('Local');
    $('#Product2').val('Import');
    $('#Product').val('');
    $("#VendorCode").tokenInput("clear");
    $("#VendorCode").val("");
    //$("input[name='Product']").change();
    BindExistingAttachment(null);
    $("#VendorCode").tokenInput("clear");
    $("#VendorCode").val("");

    //$("#VendorCode").parent().find('ul.token-input-list').remove();
    ////$(".approver").each(function () {
    ////    var role = $(this).attr("data-dept").trim();
    ////    if (role != "Creator" && role != "BU Approver" && role != "ABSQ Team" && role != "ABSQ Approver") {
    ////        $("td[data-dept$='" + role + "']").find("span").text('NA');
    ////        $("td[data-dept$='" + role + "']").find("input.hiddenuser").val('');
    ////        $("td[data-dept$='" + role + "']").find("input.hiddenusername").val('');
    ////    }
    ////});
}

function VendorCodeRemoved(ele) {

}

function BindExistingAttachment(result) {

    if (result && result != "[]") {
        $("#existingAttachmentfrommodel").html(result);
        $("#existingAttachmentfrommodel").addClass("disabled");
        setTimeout(function () {
            DisableFormItems();
        }, 1000);
        BindArtwork();
    }
    else {
        //$("#existingAttachmentfrommodel").html("");
        //if ($("#ItemCode").val() != '') {
        ////AlertModal("Validation", 'Please go to view Artwork which is available in navigation bar under Add Artwork. Raise request to the ABSQ team to upload artwork files. Follow this tab for raising a change request', true, ClearAll);
        //    AlertModal("Validation", 'Please follow "View/Request artwork additional details" tab for viewing artworks without any changes. Follow this tab for raising a change request', true, ClearAll);
        //}

        $("#existingAttachmentfrommodel").html("");
        $("#existingAttachmentfrommodel").addClass("disabled");
        setTimeout(function () {
            DisableFormItems();
        }, 1000);
    }

    bindArtworkAttachments();
}

function BindApprover() {
    // var BusinessUnitvalue = $("#BusinessUnit").val();
    var BusinessUnitvalue = $("#RequesterBusinessUnit").val();
    var DomesticImportedValue = $("#RequesterDomesticOrImported").val();
    $(Approverlist).each(function (i, item) {
        if (item.BusinessUnit == BusinessUnitvalue && item.DomesticOrImported == DomesticImportedValue) {
            $(".approver").each(function () {
                var role = $(this).attr("data-dept");
                if (role != "Creator" && role != "ABSQ Team" && role != "ABSQ Approver") {
                    //if (role != 'Creator' && role != "BU Approver" && role.indexOf("ABSQ") == 0) {
                    if (item[role.replace(/\s+/g, '') + "ApproverName"]) {
                        $("td[data-dept$='" + role + "']").find("span").text(item[role.replace(/\s+/g, '') + "ApproverName"]);
                        $("td[data-dept$='" + role + "']").find("input.hiddenuser").val(item[role.replace(/\s+/g, '') + "Approver"]);
                        $("td[data-dept$='" + role + "']").find("input.hiddenusername").val(item[role.replace(/\s+/g, '') + "ApproverName"]);
                    }
                    else if (role == "BU Approver") {
                        $("td[data-dept$='" + role + "']").find("span").text(item[role.replace(/\s+/g, '') + "Name"]);
                        $("td[data-dept$='" + role + "']").find("input.hiddenuser").val(item[role.replace(/\s+/g, '')]);
                        $("td[data-dept$='" + role + "']").find("input.hiddenusername").val(item[role.replace(/\s+/g, '') + "Name"]);
                    }
                    else {
                        if (role == 'Creator' || role.indexOf("ABSQ") != -1) {
                        } else {
                            $("td[data-dept$='" + role + "']").find("span").text('NA');
                        }
                    }
                }
            });
        }
    });
}

function BindMarkettingUser() {
    var BusinessUnitvalue = $("#BusinessUnit").val();
    var DomesticImportedValue = $("#DomesticOrImported").val();
    $(Approverlist).each(function (i, item) {
        if (item.BusinessUnit == BusinessUnitvalue && item.DomesticOrImported == DomesticImportedValue) { //
            $(".approver").each(function () {
                var role = $(this).attr("data-dept");
                if (role == "Marketing Level 1") {
                    //if (role != 'Creator' && role != "BU Approver" && role.indexOf("ABSQ") == 0) {
                    $("td[data-dept$='Marketing Level 1']").find("span").text(item.RequesterName);
                    $("td[data-dept$='Marketing Level 1']").find("input.hiddenuser").val(item.Requester);
                    $("td[data-dept$='Marketing Level 1']").find("input.hiddenusername").val(item.RequesterName);
                }
                if (role == "Marketing Level 2") {
                    //if (role != 'Creator' && role != "BU Approver" && role.indexOf("ABSQ") == 0) {
                    $("td[data-dept$='Marketing Level 2']").find("span").text(item.BUApproverName);
                    $("td[data-dept$='Marketing Level 2']").find("input.hiddenuser").val(item.BUApprover);
                    $("td[data-dept$='Marketing Level 2']").find("input.hiddenusername").val(item.BUApproverName);
                }
            });
            ////For CP only, get marketing level users from columns(Marketing level 1 approver, Marketing level 2 approver) of Existing Approver Master
            //if (item.BusinessUnit == "CP") {
            //    $(".approver").each(function () {
            //        var role = $(this).attr("data-dept");
            //        if (role == "Marketing Level 1") {
            //            //if (role != 'Creator' && role != "BU Approver" && role.indexOf("ABSQ") == 0) {
            //            $("td[data-dept$='Marketing Level 1']").find("span").text($('#hdnMrkLevel1Name').val());
            //            $("td[data-dept$='Marketing Level 1']").find("input.hiddenuser").val($('#hdnMrkLevel1').val());
            //            $("td[data-dept$='Marketing Level 1']").find("input.hiddenusername").val($('#hdnMrkLevel1Name').val());
            //        }
            //        if (role == "Marketing Level 2") {
            //            //if (role != 'Creator' && role != "BU Approver" && role.indexOf("ABSQ") == 0) {
            //            $("td[data-dept$='Marketing Level 2']").find("span").text($('#hdnMrkLevel2Name').val());
            //            $("td[data-dept$='Marketing Level 2']").find("input.hiddenuser").val($('#hdnMrkLevel2').val());
            //            $("td[data-dept$='Marketing Level 2']").find("input.hiddenusername").val($('#hdnMrkLevel2Name').val());
            //        }
            //    });
            //}

            if (item.BusinessUnit == "CP-KAP") {
                $(".approver").each(function () {
                    var role = $(this).attr("data-dept");
                    if (role == "Marketing Level 1") {
                        //if (role != 'Creator' && role != "BU Approver" && role.indexOf("ABSQ") == 0) {
                        $("td[data-dept$='Marketing Level 1']").find("span").text($('#hdnKAPMrkLevel1Name').val());
                        $("td[data-dept$='Marketing Level 1']").find("input.hiddenuser").val($('#hdnKAPMrkLevel1').val());
                        $("td[data-dept$='Marketing Level 1']").find("input.hiddenusername").val($('#hdnKAPMrkLevel1Name').val());
                    }
                    if (role == "Marketing Level 2") {
                        //if (role != 'Creator' && role != "BU Approver" && role.indexOf("ABSQ") == 0) {
                        $("td[data-dept$='Marketing Level 2']").find("span").text($('#hdnKAPMrkLevel2Name').val());
                        $("td[data-dept$='Marketing Level 2']").find("input.hiddenuser").val($('#hdnKAPMrkLevel2').val());
                        $("td[data-dept$='Marketing Level 2']").find("input.hiddenusername").val($('#hdnKAPMrkLevel2Name').val());
                    }
                });
            }
            else if (item.BusinessUnit == "CP-DAP") {
                $(".approver").each(function () {
                    var role = $(this).attr("data-dept");
                    if (role == "Marketing Level 1") {
                        //if (role != 'Creator' && role != "BU Approver" && role.indexOf("ABSQ") == 0) {
                        $("td[data-dept$='Marketing Level 1']").find("span").text($('#hdnDAPMrkLevel1Name').val());
                        $("td[data-dept$='Marketing Level 1']").find("input.hiddenuser").val($('#hdnDAPMrkLevel1').val());
                        $("td[data-dept$='Marketing Level 1']").find("input.hiddenusername").val($('#hdnDAPMrkLevel1Name').val());
                    }
                    if (role == "Marketing Level 2") {
                        //if (role != 'Creator' && role != "BU Approver" && role.indexOf("ABSQ") == 0) {
                        $("td[data-dept$='Marketing Level 2']").find("span").text($('#hdnDAPMrkLevel2Name').val());
                        $("td[data-dept$='Marketing Level 2']").find("input.hiddenuser").val($('#hdnDAPMrkLevel2').val());
                        $("td[data-dept$='Marketing Level 2']").find("input.hiddenusername").val($('#hdnDAPMrkLevel2Name').val());
                    }
                });
            }
            else if (item.BusinessUnit == "CP-FANS") {
                $(".approver").each(function () {
                    var role = $(this).attr("data-dept");
                    if (role == "Marketing Level 1") {
                        //if (role != 'Creator' && role != "BU Approver" && role.indexOf("ABSQ") == 0) {
                        $("td[data-dept$='Marketing Level 1']").find("span").text($('#hdnFANSMrkLevel1Name').val());
                        $("td[data-dept$='Marketing Level 1']").find("input.hiddenuser").val($('#hdnFANSMrkLevel1').val());
                        $("td[data-dept$='Marketing Level 1']").find("input.hiddenusername").val($('#hdnFANSMrkLevel1Name').val());
                    }
                    if (role == "Marketing Level 2") {
                        //if (role != 'Creator' && role != "BU Approver" && role.indexOf("ABSQ") == 0) {
                        $("td[data-dept$='Marketing Level 2']").find("span").text($('#hdnFANSMrkLevel2Name').val());
                        $("td[data-dept$='Marketing Level 2']").find("input.hiddenuser").val($('#hdnFANSMrkLevel2').val());
                        $("td[data-dept$='Marketing Level 2']").find("input.hiddenusername").val($('#hdnFANSMrkLevel2Name').val());
                    }
                });
            }
            else if (item.BusinessUnit == "CP-LTG") {
                $(".approver").each(function () {
                    var role = $(this).attr("data-dept");
                    if (role == "Marketing Level 1") {
                        //if (role != 'Creator' && role != "BU Approver" && role.indexOf("ABSQ") == 0) {
                        $("td[data-dept$='Marketing Level 1']").find("span").text($('#hdnLTGMrkLevel1Name').val());
                        $("td[data-dept$='Marketing Level 1']").find("input.hiddenuser").val($('#hdnLTGMrkLevel1').val());
                        $("td[data-dept$='Marketing Level 1']").find("input.hiddenusername").val($('#hdnLTGMrkLevel1Name').val());
                    }
                    if (role == "Marketing Level 2") {
                        //if (role != 'Creator' && role != "BU Approver" && role.indexOf("ABSQ") == 0) {
                        $("td[data-dept$='Marketing Level 2']").find("span").text($('#hdnLTGMrkLevel2Name').val());
                        $("td[data-dept$='Marketing Level 2']").find("input.hiddenuser").val($('#hdnLTGMrkLevel2').val());
                        $("td[data-dept$='Marketing Level 2']").find("input.hiddenusername").val($('#hdnLTGMrkLevel2Name').val());
                    }
                });
            }
        } //
    });
}

function BindArtwork() {
    var artworkTypeOptions = $("#strArtworkType").val();
    if (artworkTypeOptions != "") {
        artworkTypeOptions = artworkTypeOptions.toLowerCase();

        $('#ArtworkTypeList').multiselect('deselectAll', false);
        $('#ArtworkTypeList').multiselect('updateButtonText');

        //if ($.trim(artworkTypeOptions) != "") {
        //    $('#ArtworkTypeList').multiselect('select', artworkTypeOptions.split(","));
        //}
        //var selectedOptions = $('#ArtworkTypeList option:selected');
        $('#ArtworkTypeList option').each(function () {
            !$(this).is(':selected')
            var input = $('.artworktypemultiselect').find('input[value="' + $(this).val() + '"]');
            //var input = $('input[value="' + $(this).val() + '"]');
            var enableoption = artworkTypeOptions.split(",")
            if ($.inArray(input.val().toLowerCase(), enableoption) != -1) {
                input.prop('disabled', false);
                input.removeProp('disabled')
                input.parent('li').removeClass('disabled');
                $(input).parent().parent().show();
            } else {
                //input.remove();
                $(input).parent().parent().hide();
                //input.prop('disabled', true);
                //input.parent('li').addClass('disabled');
            }
        });
        if ($.trim($("#ArtworkTypeList").attr("data-selected")) != "") {
            $('#ArtworkTypeList').multiselect('select', $("#ArtworkTypeList").attr("data-selected").split(","));
        }
    }
    $("#ArtworkTypeList").on("change", function () {
        $('#ArtworkType').val($("#ArtworkTypeList").val());
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
            BindAttachment(fileAttachmentId, divId, 1, $('input[name="' + strArtworkTypecode + '"]').val(), fileSize);
        }
    });
}

function getQueryStringParameterByName(name) {
    name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
        results = regex.exec(location.search);
    return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
}

function CheckInprogress(itemcode) {
    if ($("input[id='ReferenceNo']").val() != '' || getQueryStringParameterByName("IsRetrieve") == "true") {
        return;
    }
    AjaxCall({
        url: "/ExistingArtwork/CheckInprogress?itemCode=" + itemcode,
        httpmethod: "GET",
        sucesscallbackfunction: function (result) {
            if (result == "False") {
                var errMessage = "Item code - " + itemcode + " is in progress.";
                AlertModal('Validation', errMessage, true, ClearAll);
            }
        }
    });
}

function ClearAll() {
    ItemCodeRemoved("#ItemCode");
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
        } else {
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

function OnDelete(ele) {
    var Id = $('#ListDetails_0__ItemId').val();
    console.log("Id = " + Id);
    ConfirmationDailog({
        title: "Delete Request", message: "Are you sure to 'Delete'?", id: Id, url: "/ExistingArtwork/DeleteArwork", okCallback: function (id, data) {
            ShowWaitDialog();
            if (data.IsSucceed) {
                AlertModal("Success", ParseMessage(data.Messages), true);
            }
            else {
                AlertModal("Error", ParseMessage(data.Messages), true)
            }
        }
    });
}