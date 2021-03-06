import React, { Component } from 'react';
import Webcam from 'react-webcam';

class CameraComponent extends Component {
  constructor(props) {
    super(props);
    this.state = {
      image: ''
    };

    this.checkFaceRecognitionForMatch = this.checkFaceRecognitionForMatch.bind(
      this
    );
  }

  setRef = webcam => {
    this.webcam = webcam;
  };

  capture = () => {
    this.state.image = this.webcam.getScreenshot();
    this.checkFaceRecognitionForMatch();
  };

  navigateToGreet() {
    this.props.history.push({
      pathname: '/greet',
      state: {
        firstName: this.state.firstName,
        lastName: this.state.lastName,
        company: this.state.company,
        email: this.state.email
      }
    });
  }

  navigateToCheckOut() {
    this.props.history.push({
      pathname: '/checkout',
      state: {
        email: this.state.email
      }
    });
  }

  navigateToForm() {
    this.props.history.push({
      pathname: '/form',
      state: {
        image: this.state.image
      }
    });
  }

  checkFaceRecognitionForMatch() {
    fetch(location.protocol + '//' + location.host +'/api/ACS/DetectAndIdentifyFace', {
      method: 'POST',
      body: JSON.stringify(this.state),
      headers: {
        'Content-type': 'application/json; charset=UTF-8'
      }
    })
      .then(response => response.json())
      .catch(error => console.error('Error:', error))
      .then(response => {
        const parsedJson = JSON.parse(response);
        if (parsedJson.registered == 'true') {
          if (parsedJson.checkedIn == 'true') {
            this.state = {
              email: parsedJson.email
            };
            this.navigateToCheckOut();
          } else {
            this.state = {
              firstName: parsedJson.firstName,
              lastName: parsedJson.lastName,
              company: parsedJson.company,
              email: parsedJson.email
            };
            this.navigateToGreet();
          }
        } else {
          this.navigateToForm();
        }
      });
  }

  render() {
    const videoConstraints = {
      width: 1280,
      height: 720,
      facingMode: 'user'
    };

    return (
      <div>
        <Webcam
          audio={false}
          height={500}
          ref={this.setRef}
          screenshotFormat="image/jpeg"
          width={500}
          videoConstraints={videoConstraints}
          style={styles.camera}
        />
        <button onClick={this.capture} style={styles.snapButton}>
          Capture photo
        </button>
      </div>
    );
  }
}

const styles = {
  camera: {
    width: '100%',
    margin: '8px 0'
  },
  snapButton: {
    margin: 'auto',
    display: 'block'
  }
};

export default CameraComponent;
