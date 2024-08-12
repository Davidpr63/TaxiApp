import DasboardNavbar from './DashboardNavbar';
import React, { useState, useEffect } from 'react';
//import { useNavigate  } from 'react-router-dom';
import { jwtDecode } from 'jwt-decode';
import apiService from '../Services/ApiService';
import { toast, ToastContainer  } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import '../styles/UserData.css'; 
function UserDataPage() {
    const [user, setUser] = useState(null);
  //  const [verificationStatus, setStatus] = useState('-');
    const [userData, setUserData] = useState({
        id:'',
        firstname: '',
        lastname: '',
        username: '',
        email: '',
        password: '',
        confirmPassword: '',
        dateOfBirth: '',
        address: '',
        imageUrl: '',
        verificationStatus: '*'
    });
    const [imageFile, setImage] = useState(null);
    const [imageUrl, setImageUrl] = useState('');
    const [updatedUser, setUpdatedUser] = useState({
        firstname: '',
        lastname: '',
        username: '',
        email: '',
        password: '',
        confirmPassword: '',
        dateOfBirth: '',
        address: '',
        image: ''
       
    });
    useEffect(() => {
      const fetchUserData = async () => {
        try {
          const token = localStorage.getItem('token'); // Pretpostavljam da čuvaš token u localStorage
          if (token) {
              // Dekodiraj token da bi dobio podatke o korisniku
              const decodedToken = jwtDecode(token);
              setUser(decodedToken);
              
              console.log("token");
              console.log(decodedToken);
              const userDataFromToken = JSON.parse(decodedToken.sub);
              console.log("Parsed user data:", userDataFromToken);
             
              setUserData({
                  id : userDataFromToken.RowKey,
                  firstname: userDataFromToken.Firstname || '',
                  lastname: userDataFromToken.Lastname || '',
                  username: userDataFromToken.Username || '',
                  email: userDataFromToken.Email || '',
                  dateOfBirth: userDataFromToken.DateOfBirth || '',
                  address: userDataFromToken.Address || '',
                  imageUrl: userDataFromToken.ImageUrl || '',
                  verificationStatus:userDataFromToken.VerifcationStatus
              });
          } else {
              console.log("Token not found");
          }
      } catch (error) {
          console.error('Failed to fetch user data:', error);
      }
      };
  
      fetchUserData();
    }, []);
    useEffect(() => {
        // Construct the URL for the blob in Azurite
        const blobUrl = 'http://127.0.0.1:10000/devstoreaccount1/userimages/a2950682-aa5f-4de6-9884-40dc42118880.png';
        setImageUrl(blobUrl);
      }, []);
     
  const handleImageChange = (e) => {
    const file = e.target.files[0];
     
      setImage(file);
  
 
  };
   const handleInputChange = (e) => {
    const { name, value } = e.target;
    setUpdatedUser({ ...updatedUser, [name]: value });    
    console.log(updatedUser)
    };
  const handleUpdateUser = async () => {
    console.log("dasd")
    const formData = new FormData();
      formData.append('UserId', userData.id);
      formData.append('Firstname', updatedUser.firstname);
      formData.append('Lastname', updatedUser.lastname);
      formData.append('Username', updatedUser.username);
      formData.append('Email', updatedUser.email);
      formData.append('Password', updatedUser.password);
      formData.append('ConfirmPassword', updatedUser.confirmPassword);
      formData.append('DateOfBirth', updatedUser.dateOfBirth);
      formData.append('Address', updatedUser.address);
      formData.append('Image', imageFile);
      console.log(imageFile);

      const data = await apiService.updateUser(formData)
      if (data.success) {
          localStorage.setItem('token', data.token);
          toast.success('You have successfully updated your data!', {
          position: 'top-right',
          autoClose: 2000,  
          hideProgressBar: false,
          closeOnClick: true,
          pauseOnHover: true,
          draggable: true,
      });
      
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
      
      
  };
  const driversVerification = async () => {
    console.log("user id - ", userData.id);
    const data = await apiService.handleDriversVerification(userData.id)
    if (data.success) {
        
        toast.success('You have successfully applied to become driver!', {
        position: 'top-right',
        autoClose: 2000,  
        hideProgressBar: false,
        closeOnClick: true,
        pauseOnHover: true,
        draggable: true,
    });
    
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
};


  return (
    <div>
       <DasboardNavbar/>
      <div className="register-card">
         
        <img src={userData.imageUrl} alt="User profile" className="profile-image" />
           
        <input
          type="text"
          name = "firstname"
          placeholder={userData.firstname}
          value={updatedUser.firstname}
          onChange={handleInputChange}
           
        />
        
       <input
          type="text"
          name = "lastname"
          placeholder={userData.lastname}
          value={updatedUser.lastname}
          onChange={handleInputChange}
        
        />
         <input
          type="text"
          name = "username"
          placeholder={userData.username}
          value={updatedUser.username}
          onChange={handleInputChange}
        
        />
        <input
          type="email"
          name = "email"
          placeholder={userData.email}
          value={updatedUser.email}
          onChange={handleInputChange}
        
        />
        <input
          type="password"
          name = "password"
          placeholder= "New password"
          value={updatedUser.password}
          onChange={handleInputChange}
           
        />
        <input
          type="password"
          name = "confirmPassword"
          placeholder="Confirm new password"
          value={updatedUser.confirmPassword} 
          onChange={handleInputChange}
           
        />
        <input
          type="text"
          name = "address"
          placeholder={userData.address}
          value={updatedUser.address}
          onChange={handleInputChange}
          
        />
        <input
          type="date"
          name = "dateOfBirth"
          value={userData.dateOfBirth}
           
          onChange={handleInputChange}
           
        />
        <input
          type="file"
          accept="image/*"
          onChange={handleImageChange}
          
        />
        <button onClick={handleUpdateUser}>Update</button>
        <div className="driver-application">
          <p>Do you want to be a driver?</p>
          <button
            onClick={driversVerification}
            disabled={
              userData.verificationStatus === 'In process' ||
              userData.verificationStatus === 'Approved'
            }
          >
            Apply
          </button>
          <p className='verification-status'>
            Verification status -    
            <span
              style={{
                color:
                userData.verificationStatus === 'In process'
                    ? 'yellow'
                    : userData.verificationStatus === 'Approved'
                    ? 'green'
                    : 'red',
              }}
            >
              {userData.verificationStatus}
            </span>
          </p>
        </div>
      </div>
     
      <ToastContainer />
    </div>
    
  );
}

export default UserDataPage;
