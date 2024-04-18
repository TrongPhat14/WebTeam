

// Go To Top
const toTop = document.querySelector(".js-gotop");
console.log(toTop);

window.addEventListener("scroll", () => {
    if (window.pageYOffset > 250) {
        toTop.classList.add("active");
    } else {
        toTop.classList.remove("active");
    }
})

console.log("hello friend")