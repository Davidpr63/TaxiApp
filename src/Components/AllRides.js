import apiService from '../Services/ApiService'; 
import React, { useState, useEffect } from 'react';
import DasboardNavbar from './DashboardNavbar';
import '../styles/PreviousRides.css';
 

const AllRides = () => {
    const [rides, setRides] = useState([]);
    
    useEffect(() => {
        const GetAllRides = async () => {
          try {
          
            const response = await apiService.GetAllRides();
            setRides(response.allRides);
            console.log('all rides ->',rides);
        } catch (error) {
            console.error('Failed to fetch user data:', error);
        }
        };
    
        GetAllRides();
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
                                <th>Status</th>
                         
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
                                        {ride.isActive ? 'Active' : 'Done'}
                                    </td>

                                </tr>
                            ))}
                        </tbody>
                    </table>
 
            </div>
     
        </div>
      
  
    );
}
export default AllRides;