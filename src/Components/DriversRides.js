import apiService from '../Services/ApiService'; 
import React, { useState, useEffect } from 'react';
import DasboardNavbar from './DashboardNavbar';
import '../styles/PreviousRides.css';
import { jwtDecode } from 'jwt-decode';

const DriversRides = () => {
    const [rides, setRides] = useState([]);
 
    useEffect(() => {
        const GetAllDriversRides = async () => {
          try {
            const token = localStorage.getItem('token');
            const decodedToken = jwtDecode(token);
            const userDataFromToken = JSON.parse(decodedToken.sub);
            console.log('Logged in Driver ->', userDataFromToken);
            const response = await apiService.GetAllDriversRides(userDataFromToken.RowKey);
            setRides(response.driversRides);
            console.log(rides);
        } catch (error) {
            console.error('Failed to fetch user data:', error);
        }
        };
    
        GetAllDriversRides();
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
export default DriversRides;