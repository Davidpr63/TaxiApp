import apiService from '../Services/ApiService'; 
import React, { useState, useEffect } from 'react';
import DasboardNavbar from './DashboardNavbar';
import '../styles/PreviousRides.css';
import { jwtDecode } from 'jwt-decode';

const PreviousRides = () => {
    const [rides, setRides] = useState([]);
    const [userId, setUserId] = useState('');
    useEffect(() => {
        const GetAllPreviousRides = async () => {
          try {
            const token = localStorage.getItem('token');
            const decodedToken = jwtDecode(token);
            const userDataFromToken = JSON.parse(decodedToken.sub);
            console.log('Logged in user ->', userDataFromToken);
           
            setUserId(userDataFromToken.RowKey);
            console.log('logged in userid ->', userId);
            const response = await apiService.GetAllPreviousRides(userDataFromToken.RowKey);
            setRides(response.previousRides);
            console.log(rides);
        } catch (error) {
            console.error('Failed to fetch user data:', error);
        }
        };
    
        GetAllPreviousRides();
      }, []);
  
    
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
                         
                            </tr>
                        </thead>
                        <tbody>
                            {rides.map(ride => (
                                <tr key={ride.rowKey}>
                                    <td>{ride.pickupAddress}</td>
                                    <td>{ride.dropOffAddress}</td>
                                    <td>{ride.randomTime}</td>
                                    <td>{ride.randomPrice}</td>
                                  
                                </tr>
                            ))}
                        </tbody>
                    </table>
 
            </div>
     
        </div>
      
  
    );
}
export default PreviousRides;