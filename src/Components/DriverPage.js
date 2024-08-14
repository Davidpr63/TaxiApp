import React, { useState, useEffect } from "react";
import apiService from "../Services/ApiService";
import '../styles/DriverPage.css';
import { jwtDecode } from 'jwt-decode';
import { toast, ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import DasboardNavbar from './DashboardNavbar';

const DriverPage = () => {
    const [rides, setRides] = useState([]);
    const [driverId, setDriverId] = useState(-1);
    const [driverArrivalCountdown, setDriverArrivalCountdown] = useState(5);
    const [rideDurationCountdown, setRideDurationCountdown] = useState(5);
    const [isRideAccepted, setIsRideAccepted] = useState(false);
    const [isRideStarted, setIsRideStarted] = useState(false);

    useEffect(() => {
        const token = localStorage.getItem('token');
        const decodedToken = jwtDecode(token);
        const userDataFromToken = JSON.parse(decodedToken.sub);
        setDriverId(userDataFromToken.RowKey);
        console.log('aaa', driverId);
    }, []);

    useEffect(() => {
        const GetAllRides = async () => {
            try {
                const response = await apiService.GetAllRides();
                setRides(response.allRides);
            } catch (error) {
                console.error('Failed to fetch user data:', error);
            }
        };
        GetAllRides();
    }, []);

    const AcceptRide = async (rideId, driverId) => {
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
            setIsRideAccepted(true);
        }
        else{
            toast.error(response.respone, {
                position: 'top-right',
                autoClose: 2000,
                hideProgressBar: false,
                closeOnClick: true,
                pauseOnHover: true,
                draggable: true,
            });
        }
    };

    useEffect(() => {
        if (isRideAccepted && driverArrivalCountdown > 0) {
            const timer = setInterval(() => {
                setDriverArrivalCountdown(prevCount => prevCount - 1);
            }, 1000);

            return () => clearInterval(timer);
        } else if (driverArrivalCountdown === 0) {
            setIsRideStarted(true);
            setDriverArrivalCountdown(null);
        }
    }, [isRideAccepted, driverArrivalCountdown]);

    useEffect(() => {
        if (isRideStarted && rideDurationCountdown > 0) {
            const timer = setInterval(() => {
                setRideDurationCountdown(prevCount => prevCount - 1);
            }, 1000);

            return () => clearInterval(timer);
        } else if (rideDurationCountdown === 0) {
            setIsRideAccepted(false);
            setIsRideStarted(false);
            setDriverArrivalCountdown(5);
            setRideDurationCountdown(5);
            toast.info('Ride has ended. You can accept another ride now!', {
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
            {!isRideAccepted && !isRideStarted && <DasboardNavbar />}
            <div className="ride-table-container">
                {!isRideAccepted && (
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
                )}
                {isRideAccepted && driverArrivalCountdown > 0 && (
                    <div className="countdown">
                        <h3>Arriving at pickup location in: {driverArrivalCountdown} seconds</h3>
                    </div>
                )}
                {isRideStarted && rideDurationCountdown > 0 && (
                    <div className="countdown">
                        <h3>Ride ends in: {rideDurationCountdown} seconds</h3>
                    </div>
                )}
            </div>
            <ToastContainer />
        </div>
    );
};

export default DriverPage;
