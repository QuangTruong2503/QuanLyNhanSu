function formatCurrency(input) {
    // Lưu vị trí con trỏ trước khi thay đổi
    let selectionStart = input.selectionStart;
    let selectionEnd = input.selectionEnd;

    // Lấy giá trị hiện tại và loại bỏ dấu phẩy
    let value = input.value.replace(/,/g, '');

    // Nếu giá trị là số, định dạng lại
    if (!isNaN(value) && value !== "") {
        input.value = parseFloat(value).toLocaleString('vi-VN');
    } else {
        input.value = "";
    }

    // Khôi phục vị trí con trỏ sau khi định dạng
    setTimeout(() => {
        input.setSelectionRange(selectionStart, selectionEnd);
    }, 0);
}

function removeFormatting() {
    let input = document.getElementById("bonusAmountInput");
    input.value = input.value.replace(/,/g, '');
}
