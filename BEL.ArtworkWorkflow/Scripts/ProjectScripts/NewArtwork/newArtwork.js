$(document).ready(function () {
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

    //Approver USer based on Business Unit Chage Start


    $("select#BusinessUnit").off("change").on("change", function () {
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

    $("select#DomesticOrImported").off("change").on("change", function () {
        BindApprover();
    }).change();




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


    //For QA
    //$('#QAUserList').multiselect({
    //    onChange: function (option, checked) {
    //        // Get selected options.

    //        var selectedOptions = $('#QAUserList option:selected');

    //        $("input[type='hidden'][name='QAUser']").val($('select#QAUserList').val());
    //        $("input[type='hidden'][name='QAUserName']").val(GetMultiselectValue(selectedOptions));

    //        if (selectedOptions.length >= 3) {
    //            // Disable all other checkboxes.
    //            var nonSelectedOptions = $('#QAUserList option').filter(function () {
    //                return !$(this).is(':selected');
    //            });

    //            nonSelectedOptions.each(function () {
    //                var input = $('input[value="' + $(this).val() + '"]');
    //                input.prop('disabled', true);
    //                input.parent('li').addClass('disabled');
    //            });
    //        }
    //        else {
    //            // Enable all checkboxes.
    //            $('#QAUserList option').each(function () {
    //                var input = $('input[value="' + $(this).val() + '"]');
    //                input.prop('disabled', false);
    //                input.parent('li').addClass('disabled');
    //            });
    //        }
    //    }
    //});

    $('#ArtworkTypeList').multiselect({
        includeSelectAllOption: true,
    });

    var selectedArtworkType = $("#ArtworkTypeList").attr("data-selected");
    if ($.trim(selectedArtworkType) != "") {
        $('#ArtworkTypeList').multiselect('select', selectedArtworkType.split(","));

        var selectedOptions = $('#ArtworkTypeList option:selected');
        selectedOptions.each(function () {
            var input = $('.artworktypemultiselect').find('input[value="' + $(this).val() + '"]');
            var totalelement = $.grep(ArtworkTypeList, function (v) {
                return v.Title === input.val() && v.IsDefaultSelected == true;
            });
            if (totalelement.length > 0) {
                input.prop('disabled', true);
                input.parent('li').addClass('disabled');
            }
        });

    }

    $("#ArtworkTypeList").on("change", function () {

        $('#ArtworkType').val($("#ArtworkTypeList").val());
    }).change();

    //if ($.trim($("#QAUserList").attr("data-selected")) != "") {
    //    $('#QAUserList').multiselect('select', $("#QAUserList").attr("data-selected").split(","));
    //}

    BindValueValidation();
});

function GetMultiselectValue(options) {
    var selected = '';
    options.each(function () {
        var label = ($(this).attr('label') !== undefined) ? $(this).attr('label') : $(this).text();
        selected += label + ",";
    });
    return selected.substr(0, selected.length - 1);
}

function bindArtworkAttachments() {
    $(".newArtworkAttachments").each(function (index) {
        var divId = $(this).find('div').attr('id');
        var fileAttachmentId = $(this).find('input').attr('id');
        var strArtworkTypecode = fileAttachmentId;
        strArtworkTypecode = fileAttachmentId.substr(0, fileAttachmentId.indexOf('.'));
        strArtworkTypecode = strArtworkTypecode + ".ArtworkTypeCode";

        fileAttachmentId = fileAttachmentId.replace(/[!"#$%&'()*+,.\/:;<=>?@[\\\]^`{|}~]/g, "\\$&");
        strArtworkTypecode = strArtworkTypecode.replace(/[!"#$%&'()*+,.\/:;<=>?@[\\\]^`{|}~]/g, "\\$&");
        var fileSize = 20 * 1024 * 1024;
        BindAttachment(fileAttachmentId, divId, 1, $('input[name="' + strArtworkTypecode + '"]').val(), fileSize);

    });


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

function VendorCodeAdded(ele, id, text) {

}

function VendorCodeRemoved(ele) {

}

function OnDelete(ele) {
    var Id = $('#ListDetails_0__ItemId').val();
    console.log("Id = " + Id);
    ConfirmationDailog({
        title: "Delete Request", message: "Are you sure to 'Delete'?", id: Id, url: "/NewArtwork/DeleteArwork", okCallback: function (id, data) {
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

function BindApprover() {
    var BusinessUnitvalue = $("#BusinessUnit option:selected").val();
    var DomesticImportedValue = $("#DomesticOrImported option:selected").val();
    if (BusinessUnitvalue != "" && DomesticImportedValue != "") {
        $(Approverlist).each(function (i, item) {
            if (item.BusinessUnit == BusinessUnitvalue && item.DomesticOrImported == DomesticImportedValue) {
                $(".approver").each(function () {
                    var role = $(this).attr("data-dept");

                    if (role != "Creator" && role != "BU Approver" && role != "ABSQ Team" && role != "ABSQ Approver") {
                        //if (role != 'Creator' && role != "BU Approver" && role.indexOf("ABSQ") == 0) {
                        if (item[role.replace(/\s+/g, '') + "Name"]) {
                            $("td[data-dept$='" + role + "']").find("span").text(item[role.replace(/\s+/g, '') + "Name"]);
                            $("td[data-dept$='" + role + "']").find("input.hiddenuser").val(item[role.replace(/\s+/g, '')]);
                            $("td[data-dept$='" + role + "']").find("input.hiddenusername").val(item[role.replace(/\s+/g, '') + "Name"]);

                        } else {
                            if (role == 'Creator' || role.indexOf("ABSQ") != -1) {
                            }
                            else if (role == "Marketing Level 1") {
                                $("td[data-dept$='" + role + "']").find("span").text(item["RequesterName"]);
                                $("td[data-dept$='" + role + "']").find("input.hiddenuser").val(item["Requester"]);
                                $("td[data-dept$='" + role + "']").find("input.hiddenusername").val(item["RequesterName"]);
                            }
                            else if (role == "Marketing Level 2") {
                                $("td[data-dept$='" + role + "']").find("span").text(item["BUHeadName"]);
                                $("td[data-dept$='" + role + "']").find("input.hiddenuser").val(item["BUHead"]);
                                $("td[data-dept$='" + role + "']").find("input.hiddenusername").val(item["BUHeadName"]);
                            }
                            else {
                                $("td[data-dept$='" + role + "']").find("span").text('NA');
                            }
                        }
                    }
                    else if (role == "BU Approver") {
                        $("td[data-dept$='" + role + "']").find("span").text(item["BUHeadName"]);
                        $("td[data-dept$='" + role + "']").find("input.hiddenuser").val(item["BUHead"]);
                        $("td[data-dept$='" + role + "']").find("input.hiddenusername").val(item["BUHeadName"]);
                    }
                });
            }
        });
    }
    else {
        $(Approverlist).each(function (i, item) {
            if (item.BusinessUnit == BusinessUnitvalue) {
                $(".approver").each(function () {
                    var role = $(this).attr("data-dept");
                    if (role != "Creator" && role != "ABSQ Team" && role != "ABSQ Approver") {
                        if (item[role.replace(/\s+/g, '') + "Name"]) {
                            $("td[data-dept$='" + role + "']").find("span").text("");
                            $("td[data-dept$='" + role + "']").find("input.hiddenuser").val("");
                            $("td[data-dept$='" + role + "']").find("input.hiddenusername").val("");

                        }
                        else if (role == "BU Approver" || role == "Marketing Level 1" || role == "Marketing Level 2") {
                            $("td[data-dept$='" + role + "']").find("span").text("");
                            $("td[data-dept$='" + role + "']").find("input.hiddenuser").val("");
                            $("td[data-dept$='" + role + "']").find("input.hiddenusername").val("");
                        }
                    }
                   
                });
            }
        });
    }
}