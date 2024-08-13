import React, { useState, useEffect } from "react";
 
import '../styles/UserPage.css';
import { toast, ToastContainer  } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import apiService from "../Services/ApiService";
import DasboardNavbar from './DashboardNavbar';
const UserPage = ({ userId }) => {
    const [pickupAddress, setPickupAddress] = useState('');
    const [dropOffAddress, setDropoffAddress] = useState('');
    const [showPopup, setShowPopup] = useState(false);
    const [randomPrice, setRandomPrice] = useState(0);
    const [randomTime, setRandomTime] = useState('');
    const [isRideConfirmed, setIsRideConfirmed] = useState(false);
    const [driverArrivalCountdown, setDriverArrivalCountdown] = useState(5);
    const [rideDurationCountdown, setRideDurationCountdown] = useState(5);
    const [isRideStarted, setIsRideStarted] = useState(false);
   
    const handleOrderClick = () => {
        // Generisanje nasumične cene i vremena
        const price = Math.floor(Math.random() * 100);  
        const minutes = Math.floor(Math.random() * 6);  
        const seconds = Math.floor(Math.random() * 60);  
        const time = `${minutes} min and ${seconds} sec`;

        // Postavljanje stanja
        setRandomPrice(price);
        setRandomTime(time);
        setShowPopup(true);
    };

    
    const handleConfirm = async () => {
        const ride = {
            pickupAddress,
            dropOffAddress,
            randomTime,
            randomPrice,
            userId
        };
        console.log('ride - ', ride)
        const response = await apiService.OrderARide(ride);
        if (response.success) {
            toast.success('You have successfully ordered a ride!', {
              position: 'top-right',
              autoClose: 2000,  
              hideProgressBar: false,
              closeOnClick: true,
              pauseOnHover: true,
              draggable: true,
          });
        setShowPopup(false);
        console.log('Ride confirmed');
        setIsRideConfirmed(true);
    }};

    const handleCancel = () => {
     
        setShowPopup(false);
    };
    useEffect(() => {
        if (isRideConfirmed && driverArrivalCountdown > 0) {
            const timer = setInterval(() => {
                setDriverArrivalCountdown(prevCount => prevCount - 1);
            }, 1000);

            return () => clearInterval(timer);
        } else if (driverArrivalCountdown === 0) {
            setIsRideStarted(true);
            setDriverArrivalCountdown(null);
        }
    }, [isRideConfirmed, driverArrivalCountdown]);

    useEffect(() => {
        if (isRideStarted && rideDurationCountdown > 0) {
            const timer = setInterval(() => {
                setRideDurationCountdown(prevCount => prevCount - 1);
            }, 1000);

            return () => clearInterval(timer);
        } else if (rideDurationCountdown === 0) {
            
            setPickupAddress('');
            setDropoffAddress('');
            setIsRideConfirmed(false);
            setDriverArrivalCountdown(5);
            setRideDurationCountdown(5);
            setIsRideStarted(false);
            toast.info('Your ride has ended. You can order another ride now!', {
                position: 'top-right',
                autoClose: 2000,  
                hideProgressBar: false,
                closeOnClick: true,
                pauseOnHover: true,
                draggable: true,
            });
        }
    }, [isRideStarted, rideDurationCountdown]);
    return (
        <div>
        <DasboardNavbar/>
        <div className="dashboard-container">
            <div className="dashboard-header">Create a ride</div>
            {!isRideConfirmed && driverArrivalCountdown === 5 && (
                <>
                    <div className="form-group">
                        <label>Pick up address</label>
                        <input 
                            type="text"
                            placeholder="Pick up address..."
                            onChange={(e) => setPickupAddress(e.target.value)}
                            required
                        />
                    </div>
                    <div className="form-group">
                        <label>Drop off address</label>
                        <input
                            type="text" 
                            placeholder="Drop off address"
                            onChange={(e) => setDropoffAddress(e.target.value)}
                            required
                        />
                    </div>
                    <div className="form-group">
                        <button onClick={handleOrderClick}>Order</button>
                        <button className="secondary-button">Cancel</button>
                    </div>
                </>
            )}

            {isRideConfirmed && driverArrivalCountdown > 0 && (
                <div className="countdown">
                    <h3>Driver arriving in: {driverArrivalCountdown} seconds</h3>
                </div>
            )}

            {isRideStarted && rideDurationCountdown > 0 && (
                <div className="countdown">
                    <h3>Ride ends in: {rideDurationCountdown} seconds</h3>
                </div>
            )}
        </div>

        {showPopup && (
            <div className="popup">
                <div className="popup-content">
                    <h3>Estimate time for a driver to arrive: {randomTime}</h3>
                    <h4>Cost: {randomPrice} euros</h4>
                    <button onClick={handleConfirm} className="confirm-btn">Confirm</button>
                    <button onClick={handleCancel} className="cancel-btn">Cancel</button>
                </div>
            </div>
        )}
         <ToastContainer/>
    </div>
    );
};

export default UserPage;
