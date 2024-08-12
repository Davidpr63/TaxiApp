 
import React, { useState } from 'react';
import { useNavigate  } from 'react-router-dom';
import { toast, ToastContainer  } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import '../styles/Register.css'; 
function RegistrationPage() {

  const [firstname, setFirstname] = useState('');
  const [lastname, setLastname] = useState('');
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [dateofbirth, setDateofbirthname] = useState('');
  const [address, setAddress] = useState('');
  const [imageUrl, setImage] = useState('');
  const navigate = useNavigate();

  const handleImageChange = (e) => {
    setImage(e.target.files[0]);
  };
  const handleRegister = () => {
      console.log(firstname);
      const formData = new FormData();
      formData.append('Firstname', firstname);
      formData.append('Lastname', lastname);
      formData.append('Username', username);
      formData.append('Email', email);
      formData.append('Password', password);
      formData.append('ConfirmPassword', confirmPassword);
      formData.append('DateOfBirth', dateofbirth);
      formData.append('Address', address);
      formData.append('Image', imageUrl);
  console.log('slikaa', imageUrl)
  fetch('http://localhost:8540/api/auth/register', {
      method: 'POST',
      body: formData
  })
  .then(response => response.json())
  .then(data => {
      console.log('server response:', data);

      if (data.success) {
        toast.success('You have successfully registered!', {
          position: 'top-right',
          autoClose: 2000,  
          hideProgressBar: false,
          closeOnClick: true,
          pauseOnHover: true,
          draggable: true,
      });
      setTimeout(() => {
        navigate('/login');
      }, 2000);
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
      <div className="register-card">
        <h2>Register</h2>
        <input
          type="text"
          placeholder="Firstname"
          value={firstname}
          onChange={(e) => setFirstname(e.target.value)}
          required
        />
        <input
          type="text"
          placeholder="Lastname"
          value={lastname}
          onChange={(e) => setLastname(e.target.value)}
          required
        />
        <input
          type="text"
          placeholder="Username"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          required
        />
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
        <input
          type="password"
          placeholder="Confirm password"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          required
        />
        <input
          type="text"
          placeholder="Address"
          value={address}
          onChange={(e) => setAddress(e.target.value)}
          required
        />
        <input
          type="date"
          
          value={dateofbirth}
          onChange={(e) => setDateofbirthname(e.target.value)}
          required
        />
        <input
          type="file"
          accept="image/*"
          onChange={handleImageChange}
          required
        />
        <button onClick={handleRegister}>Register</button>
        
      </div>
      <ToastContainer />
    </div>
    
  );
}

export default RegistrationPage;
