import '../styles/Login.css'; 
import React, {useState} from "react";
import { toast, ToastContainer  } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { useNavigate } from 'react-router-dom';

const LogInPage = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [userType, setUserType] = useState(-1);
    const navigate = useNavigate();
    const handleFacebookLogin = () => {
        window.location.href = `https://www.facebook.com/v8.0/dialog/oauth?client_id=461062726838179&redirect_uri=http://localhost:3000/&response_type=code&scope=email,public_profile`;
    };

    const handleLogin = () => {
        const formData = {
            email,
            password
        };
        console.log(email);
        fetch('http://localhost:8540/api/auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(formData)
        })
        .then(response => response.json())
        .then(data => {
            console.log('server response:', data);
    
            if (data.success) {
                 
                localStorage.setItem('token', data.token);
                
                
                navigate('/dashboard')
              
              
            } else {
                toast.error(data.error, {
                    position: 'top-right',
                    autoClose: 2000,
                    hideProgressBar: false,
                    closeOnClick: true,
                    pauseOnHover: true,
                    draggable: true,
                });
            }
        })
        .catch(error => {
            console.error('Error sending data to backend:', error);
        });
    };

    return (
        <div>
            <div className="login-card">
                <h2>Log In</h2>
                <input
                    type="email"
                    placeholder="Email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    required
                />
                <input
                    type="password"
                    placeholder="Password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                />
                <button onClick={handleLogin}>Log in</button>
                <a href="/register">Register first</a>
                <p>Or Log in with Facebook</p>
                <button onClick={handleFacebookLogin}>
                      Login with Facebook
                </button>
            </div>
            <ToastContainer/>
        </div>
        
    );
}
export default LogInPage;