document.addEventListener("DOMContentLoaded", () => {
    const loginForm = document.querySelector("form");

    if (loginForm){
        loginForm.addEventListener("submit", (event) =>{
            const emailInput = loginForm.querySelector("input[name='Email']") as HTMLInputElement;
            const passwordInput = loginForm.querySelector("input[name='Password']") as HTMLInputElement;
            
            if (!emailInput.value || !passwordInput.value){
                event.preventDefault();
                alert("Email and Password are Required");
            }
        });
    }
});