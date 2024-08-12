import apiService from '../Services/ApiService'; 
import React, { useState, useEffect } from 'react';
import DasboardNavbar from './DashboardNavbar';
import '../styles/VerificationPage.css';
import { toast, ToastContainer  } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faCheckCircle, faTimesCircle } from '@fortawesome/free-solid-svg-icons';

const VerificationPage = () => {
    const [users, setUsers] = useState([]);
    useEffect(() => {
        const GetAllVerifications = async () => {
          try {
            const response = await apiService.GetAllVerifications();
            setUsers(response.driverVerifications);
            console.log(users);
        } catch (error) {
            console.error('Failed to fetch user data:', error);
        }
        };
    
        GetAllVerifications();
      }, []);
    const ApproveUser = async (userId) => {
        console.log("driver id : ",userId);
        const response = await apiService.ApproveDriver(userId);
        if (response.success) {
         
            toast.success('You have successfully approved!', {
                position: 'top-right',
                autoClose: 2000,  
                hideProgressBar: false,
                closeOnClick: true,
                pauseOnHover: true,
                draggable: true,
            });
        }
    };
    const RejectUser = async (userId) => {
        console.log("driver id : ",userId);
        const response = await apiService.RejectRequest(userId);
        if (response.success) {
         
            toast.success('You have successfully rejected request!', {
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
            <div className="verification-table-container">
                <table className="verification-table">
                    <thead>
                    <tr>
                        <th>Firstname</th>
                        <th>Lastname</th>
                        <th>Email</th>
                        <th>Verification Status</th>
                        <th>Actions</th>
                    </tr>
                    </thead>
                    <tbody>
                    {users.map(user => (
                        <tr key={user.rowKey}>
                        <td>{user.driversName}</td>
                        <td>{user.driversLastname}</td>
                        <td>{user.driversEmail}</td>
                        <td>
                                    {user.verificationStatus}
                                    {user.verificationStatus === 'Approved' && (
                                        <FontAwesomeIcon icon={faCheckCircle} style={{ color: 'green', marginLeft: '8px' }} />
                                    )}
                                    {user.verificationStatus === 'Rejected' && (
                                        <FontAwesomeIcon icon={faTimesCircle} style={{ color: 'red', marginLeft: '8px' }} />
                                    )}
                                </td>
                        <td>
                            {user.verificationStatus !== 'Approved' && user.verificationStatus !== 'Rejected' && (
                                <>
                                    <button onClick={() => ApproveUser(user.userId)} className="approve-btn">Approve</button>
                                    <button onClick={() => RejectUser(user.userId)} className="reject-btn">Reject</button>
                                </>
                            )}
                        </td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            </div>
            <ToastContainer/>          
        </div>
      
  
    );
}
export default VerificationPage;