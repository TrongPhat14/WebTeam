// Check validation login
function validation() {
    var form = document.getElementById("form");
    var email = document.getElementById("email").value;
    var password = document.getElementById("password").value;
    var error_email = document.getElementById("error_email");
    var correct = document.querySelector(".correct");
    var incorrect = document.querySelector(".incorrect");
    var error_psw = document.getElementById("error_psw");
    var emailPattern = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/;
    var passReg = /^(?=.*[a-z])(?=.*\d)[A-Za-z\d@$!%*?& -]{8,50}$/;

    // Check format email
    if (!email) {
        form.classList.remove("valid");
        form.classList.add("invalid");
        error_email.innerHTML = "Email is required.";
        error_email.style.color = "#FF0000";
        return false;
    } else if (!email.match(emailPattern)) {
        form.classList.remove("valid");
        form.classList.add("invalid");
        error_email.innerHTML = "";
        incorrect.style.display = "flex";
        correct.style.display = "none";
        return false;
    } else {
        error_email.innerHTML = "";
        incorrect.style.display = "none";
        correct.style.display = "flex";
        error_email.style.color = "#000";
    }

    // Check format password
    if (!password) {
        form.classList.remove("valid");
        form.classList.add("invalid");
        error_psw.innerHTML = "Password is required.";
        error_psw.style.color = "#FF0000";
        return false;
    } else if (!password.match(passReg)) {
        form.classList.remove("valid");
        form.classList.add("invalid");
        error_psw.innerHTML = "Incorrect Password Format.";
        error_psw.style.color = "#FF0000";
        return false;
    } else {
        error_psw.innerHTML = "";
        error_psw.style.color = "#000";
    }

    form.classList.remove("invalid");
    form.classList.add("valid");
    return true;
}
function myFunction() {
    var x = document.getElementById("myTopnav");
    if (x.className === "topnav") {
        x.className += " responsive";
    } else {
        x.className = "topnav";
    }
}