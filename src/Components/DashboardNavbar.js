    import React, { useState, useEffect } from 'react';
    import { Link, useNavigate } from 'react-router-dom';
    import apiService from '../Services/ApiService';
    import '../styles/Navbar.css';
    import { jwtDecode } from 'jwt-decode';
    import { toast, ToastContainer  } from 'react-toastify';
    import 'react-toastify/dist/ReactToastify.css';
    const DashboardNavbar = () => {
        const navigate = useNavigate();
        const [typeOfUser, setTypeUser] = useState(-1);
        const [userImage, setUserImage] = useState('');
        useEffect(() => {
            const getTypeOfUser = async () => {
            try {
                const token = localStorage.getItem('token'); 
                if (token) {
                    
                    const decodedToken = jwtDecode(token);
                
                    const userDataFromToken = JSON.parse(decodedToken.sub);
                    console.log("Parsed user data:", userDataFromToken);
                    setTypeUser(userDataFromToken.TypeOfUser);
                    setUserImage(userDataFromToken.ImageUrl || '');
                    console.log(typeOfUser);
                    
                } else {
                    console.log("Token not found");
                }
            } catch (error) {
                console.error('Failed to fetch user data:', error);
            }
            };
        
            getTypeOfUser();
        }, []);
        const handleLogout = async () =>{
            
            const data = await apiService.LogoutButton();
                    
    
            toast.success('You have successfully logged out', {
                position: 'top-right',
                autoClose: 2000,  
                hideProgressBar: false,
                closeOnClick: true,
                pauseOnHover: true,
                draggable: true,
            });
            setTimeout(() => {
                navigate('/login')
            }, 2000);
                
        };
        return (
            <nav className="navbar">
                <div className="navbar-container">
                    <div className="user-profile">
                        <img src={userImage} alt="User profile" className="profile-image" />
                    </div>
                    {typeOfUser === 0 && (
                        <div>
                            <Link to="/dashboard" className="nav-link">Verifications</Link>
                            <Link to="/all-rides" className="nav-link">All rides</Link>
                            <Link to="/user-data" className="nav-link">User data</Link>
                        </div>
                    )}
                    {typeOfUser === 2 && (
                        <div>
                            <Link to="/drivers-rides" className="nav-link">My rides</Link>
                            <Link to="/dashboard" className="nav-link">New rides</Link>
                            <Link to="/user-data" className="nav-link">User data</Link>
                        </div>
                    )}
                    {typeOfUser === 1 && (
                        <div>
                            <Link to="/dashboard" className="nav-link">Create a ride</Link>
                            <Link to="/rides-history" className="nav-link">Ride history</Link>
                            <Link to="/user-data" className="nav-link">User data</Link>
                        </div>
                    )}
                    <button className="nav-link logout-button" onClick={handleLogout}>Log out</button>
                </div>
            </nav>
        );
    }

    export default DashboardNavbar