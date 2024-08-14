import React, { useState, useEffect } from "react";
import '../styles/UserPage.css';
import { toast, ToastContainer } from 'react-toastify';
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
    const [showRatingPopup, setShowRatingPopup] = useState(false);
    const [rating, setRating] = useState(0);

   
    const handleOrderClick = () => {
        const price = Math.floor(Math.random() * 100);  
        const minutes = Math.floor(Math.random() * 6);  
        const seconds = Math.floor(Math.random() * 60);  
        const time = `${minutes} min and ${seconds} sec`;

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
    }};

    const handleCancel = () => {
        setShowPopup(false);
    };

    useEffect(() => {
        const interval = setInterval(async () => {
            const response = await apiService.GetRideStatus(userId);
            if (response.status) {
                setIsRideConfirmed(true);
            }
        }, 2000);

        return () => clearInterval(interval);
    }, [userId]);

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
            setShowRatingPopup(true);
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
    const handleRatingSubmit = async (rating) => {
        console.log('submit rating', rating);   
        const response = await apiService.SubmitDriverRating(userId, rating);
        if (response.success) {
            setShowRatingPopup(false);
            toast.success('Thank you for rating your driver!', {
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
            {!isRideStarted && !isRideConfirmed && <DasboardNavbar />}
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
            {showRatingPopup && (
                <div className="popup">
                    <div className="popup-content">
                        <h3>Rate your driver</h3>
                        <div className="rating">
                            {[1, 2, 3, 4, 5].map((star) => (
                                <span
                                    key={star}
                                    className={star <= rating ? "star selected" : "star"}
                                    onClick={() => setRating(star)}
                                >
                                    ★
                                </span>
                            ))}
                        </div>
                        <button onClick={() => handleRatingSubmit(rating)} className="confirm-btn">Submit</button>
                    </div>
                </div>
            )}

            <ToastContainer />
        </div>
    );
};

export default UserPage;
