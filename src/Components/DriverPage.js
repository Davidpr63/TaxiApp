import React, { useState, useEffect } from "react";
import apiService from "../Services/ApiService";
import '../styles/DriverPage.css';
import { jwtDecode } from 'jwt-decode';
import { toast, ToastContainer  } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import DasboardNavbar from './DashboardNavbar';
const DriverPage = () => {
    const [rides, setRides] = useState([]);
    const [driverId, setDriverId] = useState(-1);
    useEffect(() => {
        const token = localStorage.getItem('token');
        const decodedToken = jwtDecode(token);
        const userDataFromToken = JSON.parse(decodedToken.sub);
        console.log('dsadsa', userDataFromToken);
        
        setDriverId(userDataFromToken.RowKey);
    },[]);
    useEffect(() => {
        const GetAllRides = async () => {
          try {
            const response = await apiService.GetAllRides();
            console.log('server resposne', response.allRides);
            setRides(response.allRides);
            console.log(rides);
        } catch (error) {
            console.error('Failed to fetch user data:', error);
        }
        };
    
        GetAllRides();
      }, []);
    const AcceptRide = async (rideId, driverId) => {
        console.log("driver id : ",driverId);
        console.log("ride id : ",rideId);
        const response = await apiService.AcceptRide(rideId, driverId);
        if (response.success) {
            
            toast.success('You have successfully accepted a ride!', {
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
            <div className="ride-table-container">
                <table className="ride-table">
                    <thead>
                    <tr>
                        <th>Pick up address</th>
                        <th>Drop off address</th>
                        <th>Estimated time</th>
                        <th>Estimated price</th>
                        <th>Accept</th>
                    </tr>
                    </thead>
                    <tbody>
                    {rides.map(ride => (
                        <tr key={ride.rowKey}>
                        <td>{ride.pickupAddress}</td>
                        <td>{ride.dropOffAddress}</td>
                        <td>{ride.randomTime}</td>
                        <td>{ride.randomPrice}</td>
                        <td>
                            <button onClick={() => AcceptRide(ride.rowKey, driverId)} className="accept-btn">Accept</button> 
                        </td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            </div>
            <ToastContainer/>          
        </div>
    );
};

export default DriverPage;
