const splashScreen = document.getElementById("splash-screen");
const mainContent = document.getElementById("main-content");

window.addEventListener("load", function () {
    setTimeout(() => {
        splashScreen.style.opacity = "0";
        splashScreen.style.transition = "opacity 0.5s";

        setTimeout(() => {
            splashScreen.style.display = "none"; // إخفاء الشاشة
            mainContent.style.display = "block"; // عرض المحتوى
        }, 500); // الانتظار لإنهاء تأثير الاختفاء
    }, 1000); // وقت عرض شاشة البداية
});

function toggleShield() {
    const shieldImage = document.getElementById('shieldImage');
    const currentSrc = shieldImage.src;

    if (currentSrc.includes('images/shield_without_a_key.png')) {
        shieldImage.src = 'images/shield_with_a_key.png';  // Replace with the shield with key image
    } else {
        shieldImage.src = 'images/shield_without_a_key.png';  // Replace with the shield without key image
    }
}

// the submit of the contact form
document.getElementById('CF').addEventListener('submit', function (e) {
    e.preventDefault(); // يمنع الفورم من إعادة تحميل الصفحة


    const name = document.getElementById('name').value;
    const email = document.getElementById('email').value;
    const comment = document.getElementById('comment').value;

    // fetch the data (get the link from asmaa diab)
    fetch('https://example.com/api/contact', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ name, email, comment })
    })
        .then(response => response.json())
        .then(data => {
            console.log('Success:', data);
            alert('send successfully');
        })
        .catch(error => {
            console.error('Error:', error);
            alert('something went wrong');
        });
});


// making sure the token is still valid + 
async function fetchUserData() {
    //take the token from local storage
    const token = localStorage.getItem("token") || sessionStorage.getItem("token");

    if (!token) {
        alert("Unauthorized! Please log in.");
        window.location.href = "../signup-form-19/signin.html";  // no token ? session ended > back to log in
        return;
    }

    try {
        // fetch the data once again 
        const response = await fetch("http://localhost:5147/api/auth/profile", {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Content-Type": "application/json"
            }
        });

        const userData = await response.json();  // saving the data in a variable called "userData"

        if (!response.ok) {
            alert("Failed to fetch user data!");
            return;
        }
    }
    catch (error) {
        console.error("Error fetching the API data:", error);
        alert("Something went wrong, please try again.");
    }
};
