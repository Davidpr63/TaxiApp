import React from 'react';
import FacebookLogin from 'react-facebook-login';
import { useNavigate } from 'react-router-dom';

function FacebookAuth() {
    const navigate = useNavigate();

    const responseFacebook = async (response) => {
        console.log(response);
        
  
        const accessToken = response.accessToken;
    
       
        const data = {
            accessToken: accessToken,
            userID: response.userID,
            name: response.name,
            email: response.email,
            picture: response.picture.data.url
        };
    
        try {
     
            const res = await fetch('http://localhost:8540/api/auth/facebook-login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(data)  
            });
    
            if (res.ok) {
                const result = await res.json();
                
                console.log('Korisnik uspešno prijavljen:', result);
                localStorage.setItem('token', result.token);
                navigate('/dashboard');
                // Ovdje možeš sačuvati JWT token u localStorage ili izvršiti dalju logiku
            } else {
                console.error('Greška prilikom prijave na backend:', res.statusText);
            }
        } catch (error) {
            console.error('Greška u komunikaciji sa backend-om:', error);
        }
    };

    return (
        <div>
            
            <FacebookLogin
                appId="461062726838179"  
                autoLoad={false}
                fields="name,email,picture"
                callback={responseFacebook}
                icon="fa-facebook"   
            />
        </div>
    );
}

export default FacebookAuth;
