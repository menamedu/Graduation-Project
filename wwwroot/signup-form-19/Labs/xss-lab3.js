//const loggedInUser = { username: sessionStorage.username }; // Replace with actual method to get logged-in user

var laburlexisting = "";

function createLab() {
    const data = {
        username: sessionStorage.username,
        imageName: 'low_path_traversal_image'
    };

    // Show loading state
    const button = document.getElementById('accesslab');
    button.textContent = 'Creating Lab...';
    button.disabled = true;

    fetch('/api/createLab', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Access-Control-Allow-Headers': '*',
            'accept': '*/*'
        },
        body: JSON.stringify(data)
    })
    .then(response => {
        button.textContent = 'Access Lab';
        button.disabled = false;

        if (!response.ok) {
            return response.json().then(error => {
                throw new Error(error.message || 'Failed to create lab');
            });
        }

        return response.json();
    })
    .then(data => {
        if (data.success && data.labUrl) {
            // Display the lab URL on the page
            laburlexisting = data.labUrl;
            const displayDiv = document.getElementById('labUrlDisplay');
            displayDiv.innerHTML = `Lab URL: <a href="${data.labUrl}" target="_blank">${data.labUrl}</a>`;
        } else {
            throw new Error('Failed to create lab - no URL returned');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        document.getElementById('labUrlDisplay').innerHTML = `Error: ${error.message}`;
    });
}

document.getElementById('accesslab').addEventListener('click', createLab);

/////////////////////////////////////
//termination script using
document.getElementById('terminateBtn').onclick = async function() {
    

    try {
        const response = await fetch('/api/terminateLab', {
            method: 'POST',
            headers: {
                 'Content-Type': 'application/json',
            'Access-Control-Allow-Headers': '*',
            'accept': '*/*'
            },
            body: JSON.stringify( laburlexisting ),
        });

        const data = await response.json();

        if (response.ok && data.success) {
            document.getElementById('result').innerText = "✅ Lab termination successful.";
        } else {
            document.getElementById('result').innerText = "❌ " + (data.message || 'Unknown error');
        }
    } catch (err) {
        document.getElementById('result').innerText = "❌ Error: " + err;
    }
};


/////////////////////////////////
//submit button

// Function to show the input field and submit button
        function showInput() {
            const submissionContainer = document.getElementById('submissionContainer');
            submissionContainer.style.display = 'block'; // Show the container
        }
        document.getElementById('SubmitFlag').onclick = 
        // Function to submit the flag
       async function submitFlag() {
       const token = sessionStorage.getItem('token');
            const flag = document.getElementById('flagInput').value;

            // You can add an AJAX call or form submission here to send the flag to your server
            if (flag) {
                console.log("Submitting Flag:", flag); // Replace this with your submission logic

                // Example: Using fetch to send data to a web API endpoint
              await  fetch('/api/submitLab', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                         'Authorization': `Bearer ${token}`
                    },
                    body: JSON.stringify(flag)
                })
                .then(response => {
                    if (response.ok) {
                        // Handle success
                        alert("Flag submitted successfully!");
                    } else {
                        // Handle errors
                        alert("Error submitting flag.");
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                });
            } else {
                alert("Please enter a flag before submitting.");
            }
        }
